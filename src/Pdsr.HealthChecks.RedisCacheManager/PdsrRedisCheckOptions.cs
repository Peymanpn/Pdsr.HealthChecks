using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Pdsr.HealthChecks.RedisCacheManager;

/// <summary>
/// PdsrRedisCache manager HealthCheck options.
/// </summary>
public class PdsrRedisCheckOptions
{
    
    /// <summary>
    /// Health status <see cref="HealthStatus.Degraded"/> response time Threshold in milli seconds
    /// anything above the threshold, health will be degraded <see cref="HealthStatus.Degraded"/>
    /// </summary>
    public long DegradedThreshold { get; set; }

    /// <summary>
    /// Failure Threshold for response time Threshold in milli seconds.
    /// above this limit, system will considered <see cref="HealthStatus.Unhealthy"/>
    /// </summary>
    public long UnhealthyThreshold { get; set; }
}
