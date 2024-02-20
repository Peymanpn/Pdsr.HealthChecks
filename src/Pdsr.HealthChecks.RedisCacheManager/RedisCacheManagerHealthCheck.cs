using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Pdsr.Cache;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Pdsr.HealthChecks.RedisCacheManager;

public class RedisCacheManagerHealthCheck : IHealthCheck
{
    private const string _cacheTestKey = ":health-check:";

    // expire the cache entry after 1 seconds
    private const int _cacheTestExpireSeconds = 1;
    private readonly IOptionsMonitor<PdsrRedisCheckOptions> _options;
    private readonly IRedisCacheManager _cache;
    private const string _healthIssueMessageFormat = "Degraded. Pdsr Redis Manager health check response time took {0} ms which exceeded the limit {1}. ms System health is {2}";
    private readonly string _cacheUniqueKey;

    public RedisCacheManagerHealthCheck(IOptionsMonitor<PdsrRedisCheckOptions> options, IRedisCacheManager cache)
    {
        _options = options;
        _cache = cache;
        _cacheUniqueKey = $"{_cacheTestKey}{Guid.NewGuid()}";
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var options = _options.Get(context.Registration.Name);
        string cacheKey = $"{_cacheUniqueKey}:{context.Registration.Name}";

        try
        {
            var saveValue = Stopwatch.GetTimestamp();
            if (_cache.Server.IsConnected)
            {
                await _cache.SetAsync(cacheKey, saveValue, cacheTime: _cacheTestExpireSeconds, cancellationToken: cancellationToken);
                _ = await _cache.GetAsync<long>(cacheKey, cancellationToken: cancellationToken);

                var responseTime = new TimeSpan(Stopwatch.GetTimestamp() - saveValue);
                IReadOnlyDictionary<string, object> hcResults = new ReadOnlyDictionary<string, object>(
                            new Dictionary<string, object> {
                                    {   "ping_time"                      , responseTime                      },
                                    {   "ping_time_elapsed_ms"          , responseTime.TotalMilliseconds     },
                                    {   "degraded_threshold"             , options.DegradedThreshold         },
                                    {   "unhealthy_threshold"            , options.UnhealthyThreshold        },
                    });

                // Is it above unhealthy threshold?
                if (responseTime.TotalMilliseconds > options.UnhealthyThreshold)
                {
                    string unhealthyMessage = string.Format(_healthIssueMessageFormat, responseTime.TotalMilliseconds, options.UnhealthyThreshold, HealthStatus.Unhealthy);
                    return new HealthCheckResult(context.Registration.FailureStatus, description: $"Unhealthy. Pdsr Redis Manager health check response time took {responseTime.TotalMilliseconds} ms which exceeded the limit {options.UnhealthyThreshold} ms. System is {nameof(HealthStatus.Unhealthy)}", data: hcResults);
                }
                // Is above degraded threshold
                else if (responseTime.TotalMilliseconds > options.DegradedThreshold)
                {
                    var degradedMsg = string.Format(_healthIssueMessageFormat, responseTime.TotalMilliseconds, options.DegradedThreshold, HealthStatus.Degraded);
                    //$"Degraded. Pdsr Redis Manager health check response time took {responseTime.TotalMilliseconds} ms which exceeded the limit {options.DegradedThreshold}. ms System health is {nameof(HealthStatus.Degraded)}";
                    return HealthCheckResult.Degraded(description: degradedMsg, data: hcResults);
                }
                return HealthCheckResult.Healthy(description: "Ping success.", data: hcResults);
            }
            return HealthCheckResult.Unhealthy("Cannot connect to redis");

        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }

    }
}
