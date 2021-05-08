using Microsoft.AspNetCore.Builder;
using Serilog;

namespace MyMicroservice.Common.Extensions.Telemetry
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// This will make the HTTP requests log as rich logs instead of plain text<br/>
        /// (needs to be before UseRouting, UseEndpoints and other similar configuration)
        /// </summary>
        public static IApplicationBuilder UseTelemetry(this IApplicationBuilder app) =>
            app.UseSerilogRequestLogging();
    }
}
