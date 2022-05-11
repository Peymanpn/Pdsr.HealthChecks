namespace Pdsr.HealthChecks;

/// <summary>
/// Abstraction of <see cref="ISystemHealth"/> to hold application wide health status.
/// Needs to register <see cref="ReadinessPublisher{TSysHealth}"/> through <see cref="ReadinessPublisherExtensions.RegisterReadinessPublisher{TSysHealth}(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>.
/// see: <see cref="ReadinessPublisher{TSysHealth}.ReadinessPublisher(TSysHealth, Microsoft.Extensions.Logging.ILoggerFactory, IServiceProvider)"/>
/// </summary>
public abstract class SystemHealthBase : ISystemHealth
{

    /// <inheritdoc/>
    public virtual bool OverAll { get; set; }

    /// <inheritdoc/>
    public virtual int MemoryUsedMb { get; set; }

    /// <inheritdoc/>
    public HealthReport HealthReport { get; set; }
}
