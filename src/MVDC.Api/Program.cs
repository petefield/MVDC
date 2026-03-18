using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using MVDC.Api.Identity;
using MVDC.Api.Services;
using MVDC.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MVDC API",
        Version = "v1",
        Description = "API for Mole Valley Girls Football Club"
    });
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Cosmos DB setup — configure CosmosDb:ConnectionString in appsettings.json or environment variables.
var cosmosConnectionString = builder.Configuration["CosmosDb:ConnectionString"]
    ?? throw new InvalidOperationException("CosmosDb:ConnectionString is not configured.");

var cosmosClientOptions = new CosmosClientOptions
{
    SerializerOptions = new CosmosSerializationOptions
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
    }
};

// When using the Cosmos DB Emulator (e.g. in Docker), bypass SSL certificate validation
// and use Gateway mode since Direct mode requires specific network configuration.
if (builder.Configuration.GetValue<bool>("CosmosDb:UseEmulator"))
{
    cosmosClientOptions.HttpClientFactory = () =>
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        return new HttpClient(handler);
    };
    cosmosClientOptions.ConnectionMode = ConnectionMode.Gateway;
    // The emulator advertises 127.0.0.1 in its metadata (writableLocations / readableLocations).
    // From inside Docker the API container can't reach 127.0.0.1:8081, so force the SDK to
    // only use the endpoint from the connection string (cosmosdb:8081).
    cosmosClientOptions.LimitToEndpoint = true;
}

builder.Services.AddSingleton(new CosmosClient(cosmosConnectionString, cosmosClientOptions));

// Register repositories
builder.Services.AddScoped<IRepository<Player>>(sp =>
    new CosmosRepository<Player>(sp.GetRequiredService<CosmosClient>(), sp.GetRequiredService<IConfiguration>(), "Player"));
builder.Services.AddScoped<IRepository<Coach>>(sp =>
    new CosmosRepository<Coach>(sp.GetRequiredService<CosmosClient>(), sp.GetRequiredService<IConfiguration>(), "Coach"));
builder.Services.AddScoped<IRepository<Team>>(sp =>
    new CosmosRepository<Team>(sp.GetRequiredService<CosmosClient>(), sp.GetRequiredService<IConfiguration>(), "Team"));
builder.Services.AddScoped<IRepository<Parent>>(sp =>
    new CosmosRepository<Parent>(sp.GetRequiredService<CosmosClient>(), sp.GetRequiredService<IConfiguration>(), "Parent"));
builder.Services.AddScoped<IRepository<Fixture>>(sp =>
    new CosmosRepository<Fixture>(sp.GetRequiredService<CosmosClient>(), sp.GetRequiredService<IConfiguration>(), "Fixture"));
builder.Services.AddScoped<IRepository<MatchReport>>(sp =>
    new CosmosRepository<MatchReport>(sp.GetRequiredService<CosmosClient>(), sp.GetRequiredService<IConfiguration>(), "MatchReport"));
builder.Services.AddScoped<IRepository<PlayerAvailability>>(sp =>
    new CosmosRepository<PlayerAvailability>(sp.GetRequiredService<CosmosClient>(), sp.GetRequiredService<IConfiguration>(), "PlayerAvailability"));

// ASP.NET Core Identity with custom Cosmos DB stores
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddUserStore<CosmosUserStore>()
    .AddRoleStore<CosmosRoleStore>()
    .AddDefaultTokenProviders();

// JWT Bearer Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MVDC";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MVDC";

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// NOTE: In production, set Cors:AllowedOrigins to the specific frontend domain(s).
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (!string.IsNullOrWhiteSpace(allowedOrigins))
        {
            policy.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // Fallback for development only
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Ensure the Cosmos DB database and container exist on startup.
// This is especially useful for first-time setup and Docker environments.
// Runs with retries so the app still starts even if the emulator is slow to become fully ready.
var databaseName = builder.Configuration["CosmosDb:DatabaseName"] ?? "MVDC";
var containerName = builder.Configuration["CosmosDb:ContainerName"] ?? "Items";
var cosmosClient = app.Services.GetRequiredService<CosmosClient>();

for (var attempt = 1; attempt <= 10; attempt++)
{
    try
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName, cancellationToken: cts.Token);
        await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id", cancellationToken: cts.Token);
        app.Logger.LogInformation("Cosmos DB database '{Database}' and container '{Container}' are ready.", databaseName, containerName);
        break;
    }
    catch (Exception ex) when (attempt < 10)
    {
        app.Logger.LogWarning(ex, "Cosmos DB initialization attempt {Attempt}/10 failed. Retrying in 3 seconds...", attempt);
        await Task.Delay(3000);
    }
}

// Seed default admin users (passwords should be set via environment variables in production)
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var adminSeeds = new[]
    {
        new { Email = "admin@mvgfc.co.uk", Name = "Admin", Password = app.Configuration["Seed:AdminPassword"] ?? "Admin123!" },
        new { Email = "pete.field@gmail.com", Name = "Pete Field", Password = app.Configuration["Seed:PetePassword"] ?? "1Plus2=3" },
    };

    foreach (var seed in adminSeeds)
    {
        var existing = await userManager.FindByEmailAsync(seed.Email);
        if (existing is null)
        {
            var adminUser = new ApplicationUser
            {
                Email = seed.Email,
                Name = seed.Name,
                Role = "Admin"
            };
            var result = await userManager.CreateAsync(adminUser, seed.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                app.Logger.LogInformation("Admin user '{Email}' seeded successfully.", seed.Email);
            }
            else
            {
                app.Logger.LogError("Failed to seed admin user '{Email}': {Errors}", seed.Email, string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}

// Seed Mole Valley Girls teams
using (var scope = app.Services.CreateScope())
{
    var teamRepo = scope.ServiceProvider.GetRequiredService<IRepository<Team>>();
    var teamSeeds = new[]
    {
        new { Id = "mv-u9-blacks", Name = "Mole Valley Girls U9 Blacks" },
        new { Id = "mv-u9-greens", Name = "Mole Valley Girls U9 Greens" },
    };

    foreach (var seed in teamSeeds)
    {
        var existing = await teamRepo.GetByIdAsync(seed.Id);
        if (existing is null)
        {
            await teamRepo.CreateAsync(new Team { Id = seed.Id, Name = seed.Name });
            app.Logger.LogInformation("Team '{Name}' seeded successfully.", seed.Name);
        }
    }
}

// Seed fixtures from FA Full-Time for Mole Valley Girls teams
using (var scope = app.Services.CreateScope())
{
    var fixtureRepo = scope.ServiceProvider.GetRequiredService<IRepository<Fixture>>();
    try
    {
        await FullTimeFixtureSeeder.SeedAsync(fixtureRepo, app.Logger);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Full-Time fixture seeding failed. The API will continue to start.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MVDC API v1");
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
