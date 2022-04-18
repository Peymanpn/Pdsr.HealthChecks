using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pdsr.HealthChecks
{
    public class MemoryHealthCheck : IHealthCheck
    {
        public const string Name = "memory";
        public const string AllocatedMegaBytesKey = "allocated_mb";

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
                { "allocated_bytes", allocated },
                {  AllocatedMegaBytesKey ,  allocatedMb },
                { "gen_0_collections", GC.CollectionCount(0) },
                { "gen_1_collections", GC.CollectionCount(1) },
                { "gen_2_collections", GC.CollectionCount(2) },
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
                    $">= {options.DegradedThreshold / 1024L / 1024L } MiBibytes." +
                    $"Error Threshold >= {options.UnhealthyThreshold / 1024L / 1024L } MiBi",
                exception: null,
                data: data));
        }
    }



    public static class GCInfoHealthCheckBuilderExtensions
    {
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
}
