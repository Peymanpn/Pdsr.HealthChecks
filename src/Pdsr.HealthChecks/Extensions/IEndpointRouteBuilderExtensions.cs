using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Pdsr.HealthChecks.Extensions
{
    public static class IEndpointRouteBuilderExtensions
    {
        private const string _defaultHealthEndpoint = "/health";

        public static void MapHealthChecksWithResults(this IEndpointRouteBuilder builder, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new System.ArgumentException($"'{nameof(pattern)}' cannot be null or empty.", nameof(pattern));
            }

            builder.MapHealthChecks(pattern, new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {

                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }

        public static void MapHealthChecksWithResults(this IEndpointRouteBuilder builder)
            => MapHealthChecksWithResults(builder, _defaultHealthEndpoint);
    }


    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder MapHealthChecksWithResultsEndpoints(this IApplicationBuilder builder, string pattern)
            => builder.UseEndpoints(endpoints => endpoints.MapHealthChecksWithResults(pattern));

        public static IApplicationBuilder MapHealthChecksWithResultsDeafultEndpoints(this IApplicationBuilder builder)
            => builder.UseEndpoints(endpoints => endpoints.MapHealthChecksWithResults());
    }
}
