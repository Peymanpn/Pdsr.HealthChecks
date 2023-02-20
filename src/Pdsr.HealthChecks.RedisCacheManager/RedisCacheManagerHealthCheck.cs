using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Pdsr.Cache;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Pdsr.HealthChecks.RedisCacheManager;

public class RedisCacheManagerHealthCheck : IHealthCheck
{
    private const string cacheTestKey = ":health-check:";
    private readonly IOptionsMonitor<PdsrRedisCheckOptions> _options;
    private readonly IRedisCacheManager _cache;

    public RedisCacheManagerHealthCheck(IOptionsMonitor<PdsrRedisCheckOptions> options, IRedisCacheManager cache)
    {
        _options = options;
        _cache = cache;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var options = _options.Get(context.Registration.Name);
        string cacheKey = $"{cacheTestKey}{context.Registration.Name}";

        try
        {
            var saveValue = Stopwatch.GetTimestamp();
            if (_cache.Server.IsConnected)
            {
                await _cache.SetAsync(cacheKey, saveValue, cacheTime: null, cancellationToken: cancellationToken);
                var cachedValue = await _cache.GetAsync<long>(cacheKey, cancellationToken: cancellationToken);

                var responseTime = new TimeSpan(Stopwatch.GetTimestamp() - saveValue);
                IReadOnlyDictionary<string, object> hcResults = new ReadOnlyDictionary<string, object>(
                            new Dictionary<string, object> {
                                    {   "ping_time"                      , responseTime                      },
                                    {   "ping_time_ellapsed_ms"          , responseTime.TotalMilliseconds    }
                        });
                if (responseTime.TotalMilliseconds > options.UnhealthyThreshold)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Unhealthy. Pdsr Redis Manager health check exceeded the limit {options.UnhealthyThreshold}. System is {nameof(HealthStatus.Unhealthy)}", data: hcResults);
                }
                else if (responseTime.TotalMilliseconds > options.DegradedThreshold)
                {
                    var degradedMsg = $"Degraded. Pdsr Redis Manager health check exceeded the limit {options.DegradedThreshold}. System is {nameof(HealthStatus.Degraded)}";
                    return HealthCheckResult.Degraded(description: degradedMsg, data: hcResults);
                }
                return HealthCheckResult.Healthy(description: "Ping", data: hcResults);
            }
            return HealthCheckResult.Unhealthy("Cannot connect to redis");

        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }

    }
}
