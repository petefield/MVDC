using Microsoft.Azure.Cosmos;
using MVDC.Api.Services;
using MVDC.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

// NOTE: Restrict AllowedOrigins to specific domains in production.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
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

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
