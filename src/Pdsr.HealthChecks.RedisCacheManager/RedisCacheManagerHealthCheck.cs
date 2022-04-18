using Pdsr.Cache;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Pdsr.HealthChecks.RedisCacheManager;

public class RedisCacheManagerHealthCheck : IHealthCheck
{
    private const string cacheTestKey = ":health-check:";
    private readonly IRedisCacheManager _cache;

    public RedisCacheManagerHealthCheck(IRedisCacheManager cache)
    {
        _cache = cache;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var saveValue = DateTime.UtcNow.Ticks;
            if (_cache.Server.IsConnected)
            {
                await _cache.SetAsync(cacheTestKey, saveValue, cacheTime: null, cancellationToken: cancellationToken);
                var cachedValue = await _cache.GetAsync<long>(cacheTestKey, cancellationToken: cancellationToken);

                var responseTime = new TimeSpan(DateTime.UtcNow.Ticks - saveValue);
                IReadOnlyDictionary<string, object> hcResults = new ReadOnlyDictionary<string, object>(
                            new Dictionary<string, object> {
                                    {   "PingTime"                      , responseTime                      },
                                    {   "PingTimeEllapsedMilliseconds"  , responseTime.TotalMilliseconds    }
                        });
                return HealthCheckResult.Healthy(description: "Ping", hcResults);
            }
            return HealthCheckResult.Unhealthy("Cannot connect");

        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }

    }
}
