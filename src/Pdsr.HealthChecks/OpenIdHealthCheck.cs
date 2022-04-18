using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pdsr.Hosting
{
    public class OpenIdHealthCheck : IHealthCheck
    {
        const string _openIdConfigUrl = ".well-known/openid-configuration";
        private readonly Func<HttpClient> _httpClientFactory;

        public OpenIdHealthCheck(Func<HttpClient> httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = _httpClientFactory();
                var response = await client.GetAsync(_openIdConfigUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: "Server Is Not Available");
                }
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }


    public static class IdentityHealthChecksExtensions
    {
        const string _clientName = "idsrv_hc";

        public static IHealthChecksBuilder AddOpenIdHealthCheck(this IHealthChecksBuilder builder, string name ,string url, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            string registrationName = name ?? _clientName;
            Uri idSrvUri = new Uri(url);
            builder.Services.AddHttpClient(registrationName, client => client.BaseAddress = idSrvUri);

            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp => new OpenIdHealthCheck(() => sp.GetRequiredService<IHttpClientFactory>().CreateClient(registrationName)),
                failureStatus,
                tags,
                timeout));
        }
    }
}
