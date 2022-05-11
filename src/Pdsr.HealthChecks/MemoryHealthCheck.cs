using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Pdsr.HealthChecks;

/// <summary>
/// Memory HealthCheck. <see cref="IHealthCheck"/>
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    public const string Name = "memory";
    public const string AllocatedMegaBytesKey = "allocated_mb";
    public const string AllocatedBytesKey = "allocated_bytes";
    public const string Gen0CollectionKey = "gen_0_collections";
    public const string Gen1CollectionKey = "gen_1_collections";
    public const string Gen2CollectionKey = "gen_2_collections";

    private readonly IOptionsMonitor<MemoryCheckOptions> _options;
    private readonly ILogger<MemoryHealthCheck> _logger;

    public MemoryHealthCheck(IOptionsMonitor<MemoryCheckOptions> options, ILogger<MemoryHealthCheck> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var options = _options.Get(context.Registration.Name);

        // Include GC information in the reported diagnostics.
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var allocatedMb = allocated / 1024L / 1024L;
        var data = new Dictionary<string, object>()
        {
            { AllocatedBytesKey         , allocated             },
            { AllocatedMegaBytesKey     , allocatedMb           },
            { Gen0CollectionKey         , GC.CollectionCount(0) },
            { Gen1CollectionKey         , GC.CollectionCount(1) },
            { Gen2CollectionKey         , GC.CollectionCount(2) },
        };

        HealthStatus status = (allocated < options.DegradedThreshold) ?
            HealthStatus.Healthy : context.Registration.FailureStatus;
        //report unhealthy if larger than max threshold
        if (allocated > options.UnhealthyThreshold) status = HealthStatus.Unhealthy;

        _logger.LogTrace("Memory Health. used: {allocatedMegaBytes} MiBi, data: {data}. \r\n Reports degraded status if allocated bytes " +
                ">= {Threshold} bytes. Repoty Unhealthy if allocated bytes >= {errorThreshold} ", allocatedMb, data, options.DegradedThreshold, options.UnhealthyThreshold);
        return Task.FromResult(new HealthCheckResult(
            status,
            description: "Reports degraded status if allocated bytes " +
                $">= {options.DegradedThreshold / 1024L / 1024L} MiBi." +
                $"Error Threshold >= {options.UnhealthyThreshold / 1024L / 1024L} MiBi",
            exception: null,
            data: data));
    }
}


/// <summary>
/// Extensions for registers a HealthCheck for Memory monitoring 
/// </summary>
public static class GCInfoHealthCheckBuilderExtensions
{
    /// <summary>
    /// Registers memory HealthCheck to current HealthChecks applied to <see cref="IHealthChecksBuilder"/>
    /// </summary>
    /// <param name="builder">HealthCheckBuilder</param>
    /// <param name="degradedThreshold">
    /// Used memory amount Threshold to show degraded health status. <see cref="HealthStatus.Degraded"/>.
    /// example: 52428800L for 50M. (50L * 1024L * 1024L).
    /// </param>
    /// <param name="unhealthyThreshold">Max Threshold to show <paramref name="failureStatus"/>. usually is <see cref="HealthStatus.Unhealthy"/>.
    /// Memory amount in bytes.
    /// example: 52428800L for 50M. (50L * 1024L * 1024L).</param>
    /// <param name="failureStatus">The HealthStatus when memory usage reaches <paramref name="unhealthyThreshold"/>.</param>
    /// <param name="tags">a collection of string values being used as custom tags to add to the HealthCheck</param>
    /// <returns>Returns the HealthCheckBuilder with MemoryHealthCheck added to.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws if the <paramref name="degradedThreshold"/> is zero, <paramref name="unhealthyThreshold"/> is zero or <paramref name="degradedThreshold"/> is bigger than <paramref name="unhealthyThreshold"/></exception>
    public static IHealthChecksBuilder AddMemoryHealthCheck(
        this IHealthChecksBuilder builder,
        long degradedThreshold,
        long unhealthyThreshold,
        // string name = MemoryHealthCheck.Name,
        HealthStatus? failureStatus = null,
        IEnumerable<string> tags = null
        )
    {
        if (degradedThreshold == 0) throw new ArgumentOutOfRangeException(nameof(degradedThreshold));
        if (unhealthyThreshold == 0) throw new ArgumentOutOfRangeException(nameof(unhealthyThreshold));
        if (unhealthyThreshold < degradedThreshold) throw new ArgumentOutOfRangeException("Unhealthy Threshold must be greater than the Degraded threshold");
        var name = MemoryHealthCheck.Name;
        // Register a check of type GCInfo.
        builder.AddCheck<MemoryHealthCheck>(
            name, failureStatus ?? HealthStatus.Degraded, tags);

        // Configure named options to pass the threshold into the check.
        if (degradedThreshold != 0)
        {
            builder.Services.Configure<MemoryCheckOptions>(name, options =>
            {
                options.DegradedThreshold = degradedThreshold;
                options.UnhealthyThreshold = unhealthyThreshold;
            });
        }
        return builder;
    }
}
