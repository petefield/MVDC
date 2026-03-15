using Microsoft.Azure.Cosmos;
using MVDC.Api.Services;
using MVDC.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Cosmos DB setup — configure CosmosDb:ConnectionString in appsettings.json or environment variables.
var cosmosConnectionString = builder.Configuration["CosmosDb:ConnectionString"]
    ?? throw new InvalidOperationException("CosmosDb:ConnectionString is not configured.");
builder.Services.AddSingleton(new CosmosClient(cosmosConnectionString));

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

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();
