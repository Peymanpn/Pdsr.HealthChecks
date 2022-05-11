using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Pdsr.Cache;
using System.Collections.ObjectModel;

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
            var saveValue = DateTime.UtcNow.Ticks;
            if (_cache.Server.IsConnected)
            {
                await _cache.SetAsync(cacheKey, saveValue, cacheTime: null, cancellationToken: cancellationToken);
                var cachedValue = await _cache.GetAsync<long>(cacheKey, cancellationToken: cancellationToken);

                var responseTime = new TimeSpan(DateTime.UtcNow.Ticks - saveValue);
                IReadOnlyDictionary<string, object> hcResults = new ReadOnlyDictionary<string, object>(
                            new Dictionary<string, object> {
                                    {   "ping_time"                      , responseTime                      },
                                    {   "ping_time_ellapsed_ms"          , responseTime.TotalMilliseconds    }
                        });
                if (responseTime.TotalMilliseconds > options.UnhealthyThreshold)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Unhealthy. Pdsr Redis Manager health check exceeded the limit {_options.CurrentValue.UnhealthyThreshold}. System is {nameof(HealthStatus.Unhealthy)}");
                }
                else if (responseTime.TotalMilliseconds > options.DegradedThreshold)
                {
                    var degradedMsg = $"Degraded. Pdsr Redis Manager health check exceeded the limit {_options.CurrentValue.DegradedThreshold}. System is {nameof(HealthStatus.Degraded)}";
                    return HealthCheckResult.Degraded(degradedMsg);
                }
                return HealthCheckResult.Healthy(description: "Ping", hcResults);
            }
            return HealthCheckResult.Unhealthy("Cannot connect to redis");

        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }

    }
}
