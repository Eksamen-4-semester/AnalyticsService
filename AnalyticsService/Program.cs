using AnalyticsService.Repositories;
using AnalyticsService.Repositories.Interfaces;
using MongoDB.Driver;
using NLog;
using NLog.Web;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

var builder = WebApplication.CreateBuilder(args);

var endPoint = Environment.GetEnvironmentVariable("VAULT_URL")
               ?? "https://localhost:8201/";

if (builder.Environment.IsDevelopment())
{
    endPoint = endPoint.Replace("localhost", "host.docker.internal");
}

var httpClientHandler = new HttpClientHandler();
httpClientHandler.ServerCertificateCustomValidationCallback =
    (message, cert, chain, sslPolicyErrors) => true;

IAuthMethodInfo authMethod =
    new TokenAuthMethodInfo("00000000-0000-0000-0000-000000000000");

var vaultClientSettings = new VaultClientSettings(endPoint, authMethod)
{
    Namespace = "",
    MyHttpClientProviderFunc = handler =>
        new HttpClient(httpClientHandler)
        {
            BaseAddress = new Uri(endPoint)
        }
};

var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

logger.Debug("Starting AnalyticsService");
logger.Debug("Connecting to Hashicorp Vault on: {0}", endPoint);

IVaultClient vaultClient = new VaultClient(vaultClientSettings);

try
{
    // Henter MongoDB secrets fra Vault
    logger.Debug("Getting MongoDB connection string and database name from Vault");

    Secret<SecretData> mongoSecrets = await vaultClient
        .V1.Secrets.KeyValue.V2
        .ReadSecretAsync(
            path: "mongo",
            mountPoint: "secret");

    string connectionString = mongoSecrets
        .Data.Data["MONGO_CONNECTION_STRING"]?.ToString()
        ?? throw new NullReferenceException(
            "MONGO_CONNECTION_STRING not found in Vault");

    logger.Debug("MongoDB connection string loaded from Vault");
    if (builder.Environment.IsDevelopment())
    {
        connectionString = connectionString.Replace("mongodb", "localhost");
    }
    Environment.SetEnvironmentVariable("MONGO_CONNECTION_STRING", connectionString);

    string dbName = mongoSecrets
        .Data.Data["MONGO_ANALYTICS_DB"]?.ToString()
        ?? throw new NullReferenceException(
            "MONGO_ANALYTICS_DB not found in Vault");

    logger.Debug("MongoDB database name loaded from Vault");
    Environment.SetEnvironmentVariable("MONGO_DATABASE_NAME", dbName);
}
catch (Exception e)
{
    logger.Error($"Something went wrong connecting to Vault: {e.InnerException?.Message}");
    throw;
}

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin();
    });
});

builder.Services.AddAuthorization();

builder.Logging.ClearProviders();
builder.Host.UseNLog();

// MongoDB Setup
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var cs = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("MONGO_CONNECTION_STRING environment variable is not set");
    return new MongoClient(cs);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    var databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME");
    if (string.IsNullOrWhiteSpace(databaseName))
        throw new InvalidOperationException("MONGO_DATABASE_NAME environment variable is not set");
    return mongoClient.GetDatabase(databaseName);
});

// Repositories
builder.Services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepositoryMongoDb>();
builder.Services.AddScoped<IExerciseSetRepository, ExerciseSetRepositoryMongoDb>();
builder.Services.AddScoped<ISessionExerciseRepository, SessionRepositoryMongoDb>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepositoryMongoDb>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();
app.UseCors("AllowBlazor");
app.UseAuthorization();
app.MapControllers();

app.Run();