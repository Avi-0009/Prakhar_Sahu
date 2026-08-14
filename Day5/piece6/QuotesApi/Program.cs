using Microsoft.Extensions.Http.Resilience;
using Polly;
using QuotesApi.Options;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Context;
using DotNetEnv;
using QuotesApi.Extensions;
using QuotesApi.Endpoints;
using QuotesApi.Data;
using QuotesApi.Models;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddInfrastructure(builder.Configuration);


var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

var otel = builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("QuotesApi"))
    .WithTracing(t =>
    {
        t.AddSource("QuotesApi.Custom")
            .AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddHttpClientInstrumentation();

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            t.AddOtlpExporter();
        }
    });

if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
    otel.UseAzureMonitor(options => options.ConnectionString = appInsightsConnectionString);
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddHealthChecks();
builder.Services.AddHttpClient("ExternalService")
    .AddResilienceHandler("default", b =>
    {
        b.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        })
        .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 2
        })
        .AddTimeout(TimeSpan.FromSeconds(10));
    });

var app = builder.Build();

app.Logger.LogInformation(
    "Startup: environment={Environment} azureMonitor={AzureMonitor} otlpExporter={Otlp}",
    app.Environment.EnvironmentName,
    string.IsNullOrWhiteSpace(appInsightsConnectionString) ? "disabled" : "enabled",
    string.IsNullOrWhiteSpace(otlpEndpoint) ? "disabled" : "enabled");

app.MapHealthChecks("/health");

app.Use(async (ctx, next) =>
{
    using (LogContext.PushProperty("TraceId", ctx.TraceIdentifier))
    {
        await next(ctx);
    }
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var seedEmail = app.Configuration["Seed:AdminEmail"];
        var seedPassword = app.Configuration["Seed:AdminPassword"];

        if (!string.IsNullOrWhiteSpace(seedEmail) && !string.IsNullOrWhiteSpace(seedPassword))
        {
            db.Users.Add(new User
            {
                Email = seedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seedPassword)
            });
            db.SaveChanges();
            app.Logger.LogInformation("Seeded initial user {Email}.", seedEmail);
        }
        else
        {
            app.Logger.LogWarning(
                "User table is empty and Seed:AdminEmail / Seed:AdminPassword are not configured, " +
                "so no user was seeded. Login will return 401 until a user exists.");
        }
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapQuoteEndpoints();

app.MapGet("/test-retry", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("ExternalService");
    var response = await client.GetAsync("https://httpstat.us/500");
    return Results.Content($"Status: {response.StatusCode}");
});

app.Run();

public partial class Program { }
