namespace Pdsr.HealthChecks;

/// <summary>
/// Global application System health.
/// If <see cref="MemoryHealthCheck"/> is registered, <see cref="MemoryUsedMb"/> will show the Application used memory.
/// see: <seealso cref="ReadinessPublisher{TSysHealth}.ReadinessPublisher(TSysHealth, Microsoft.Extensions.Logging.ILoggerFactory, IServiceProvider)"/>
/// </summary>
public interface ISystemHealth
{
    /// <summary>
    /// The latest health report happened in <see cref="ReadinessPublisher{TSysHealth}"/>
    /// </summary>
    HealthReport HealthReport { get; set; }

    /// <summary>
    /// Used memory in MB.
    /// </summary>
    int MemoryUsedMb { get; set; }

    /// <summary>
    /// Overall app is healthy or not.
    /// </summary>
    bool OverAll { get; set; }
}
