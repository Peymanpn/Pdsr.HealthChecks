namespace Pdsr.HealthChecks;

public class MemoryCheckOptions
{
    /// <summary>
    /// Warning Threshold (in bytes)
    /// anything above the threshold, health will be degraded <see cref="HealthStatus.Degraded"/>
    /// </summary>
    public long DegradedThreshold { get; set; } = 1024L * 1024L * 1024L;

    /// <summary>
    /// Failure Threshold
    /// above this limit, system will considered unhealthy <see cref="HealthStatus.Unhealthy"/>
    /// </summary>
    public long UnhealthyThreshold { get; set; }
}
