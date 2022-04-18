using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pdsr.HealthChecks;

public static class UIResponseWriter
{
    const string DEFAULT_CONTENT_TYPE = "application/json";

    private static readonly byte[] emptyResponse = new byte[] { (byte)'{', (byte)'}' };
    private static readonly Lazy<JsonSerializerOptions> options = new Lazy<JsonSerializerOptions>(() => CreateJsonOptions());

    public static async Task WriteHealthCheckUIResponse(HttpContext httpContext, HealthReport report)
    {
        if (report != null)
        {
            httpContext.Response.ContentType = DEFAULT_CONTENT_TYPE;

            var uiReport = UIHealthReport
                .CreateFrom(report);

            using var responseStream = new MemoryStream();

            await JsonSerializer.SerializeAsync(responseStream, uiReport, options.Value);
            await httpContext.Response.BodyWriter.WriteAsync(responseStream.ToArray());
        }
        else
        {
            await httpContext.Response.BodyWriter.WriteAsync(emptyResponse);
        }
    }
    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        options.Converters.Add(new JsonStringEnumConverter());

        //for compatibility with older UI versions ( <3.0 ) we arrange
        //timespan serialization as s
        // options.Converters.Add(new TimeSpanConverter());

        return options;
    }
}
