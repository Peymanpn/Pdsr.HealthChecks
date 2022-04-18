using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Pdsr.HealthChecks.RedisCacheManager;

public static class IRedisCacheManagerHealthChecksExtensions
{
    public static IHealthChecksBuilder AddRedisCacheManager(
        this IHealthChecksBuilder builder,
        string name,
        HealthStatus? failureStatus = null,
        IEnumerable<string> tags = null,
        TimeSpan? responseTimeLimits = null)
    {
        return builder.AddCheck<RedisCacheManagerHealthCheck>(
             name, failureStatus ?? HealthStatus.Degraded, tags);
    }
}
