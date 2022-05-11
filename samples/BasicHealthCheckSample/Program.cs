using BasicHealthCheckSample;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Pdsr.HealthChecks;
using Pdsr.HealthChecks.Extensions;
using Pdsr.HealthChecks.RedisCacheManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRedisCacheManager("redis", 6379);

builder.Services.AddHealthChecks()
    .AddCheck("some_health_check", c =>
    {
        return HealthCheckResult.Healthy("all is good", new Dictionary<string, object>
        {
            { "date_utc",DateTimeOffset.UtcNow }
        });

    }, new[] { "foo", "bar" })
    .AddMemoryHealthCheck(
        degradedThreshold: 50 * 1024L * 1024L, // shows degraded at this degradedThreshold
       unhealthyThreshold: 100 * 2048L * 2048L,  // max Threshold to show failureStatus (unhealthy)
       failureStatus: HealthStatus.Unhealthy,
       tags: new string[] { "memory", "ram" })
    .AddRedisCacheManager(50, 100);


builder.Services.RegisterReadinessPublisher<MyHealthStatus>();

var app = builder.Build();


app.MapGet("/", () => "Hello World!");

app.MapHealthChecksWithResults();
app.Run();
