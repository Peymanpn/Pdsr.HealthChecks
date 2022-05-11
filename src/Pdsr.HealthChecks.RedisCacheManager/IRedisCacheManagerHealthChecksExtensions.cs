using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Pdsr.HealthChecks.RedisCacheManager;

/// <summary>
/// Pdsr RedisCacheManager registration extensions
/// </summary>
public static class RedisCacheManagerHealthChecksExtensions
{
    /// <summary>
    /// Registers the <see cref="Pdsr.Cache.IRedisCacheManager"/> HealthCheck to monitor Redis availability.
    /// Important: instance of <see cref="Pdsr.Cache.IRedisCacheManager"/> must already registered.
    /// </summary>
    /// <param name="builder">HealthCheckBuilder instance.</param>
    /// <param name="degradedResponseLimit">Threshold for redis response time in milli seconds to state <see cref="HealthStatus.Degraded"/></param>
    /// <param name="unhealthyThreshold">Threshold for redis response time in milli seconds to considered failure.</param>
    /// <param name="failureStatus">State when <paramref name="unhealthyThreshold"/>exceeds.</param>
    /// <param name="name">Name of the healthcheck. default: "redis"</param>
    /// <param name="failureStatus"><see cref="HealthStatus"/> in case of failure. deaful: <see cref="HealthStatus.Unhealthy"/></param>
    /// <param name="tags">a collection of string values being used as custom tags to add to the HealthCheck</param>
    /// <returns>Configured HealthCheckBuilder with RedisCacheManager added to.</returns>
    public static IHealthChecksBuilder AddRedisCacheManager(
        this IHealthChecksBuilder builder,
        long degradedResponseLimit,
        long unhealthyThreshold,
        string name = "redis",
        HealthStatus failureStatus = HealthStatus.Unhealthy,
        IEnumerable<string> tags = null)
    {
        builder.Services.Configure<PdsrRedisCheckOptions>(name, healthCheckOptions =>
        {
            healthCheckOptions.UnhealthyThreshold = unhealthyThreshold;
            healthCheckOptions.DegradedThreshold = degradedResponseLimit;
        });
        return builder.AddCheck<RedisCacheManagerHealthCheck>(name, failureStatus, tags);
    }
}
