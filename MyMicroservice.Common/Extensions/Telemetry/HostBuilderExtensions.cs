using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using System.Diagnostics;

namespace MyMicroservice.Common.Extensions.Telemetry
{
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Adds Serilog with several sinks
        /// See <see href="https://jkdev.me/asp-net-core-serilog"/> for more information on how to configure Serilog
        /// </summary>
        public static IHostBuilder AddTelemetry(this IHostBuilder hostBuilder, string? serviceName)
        {
            return hostBuilder.UseSerilog((context, config) =>
            {
                string? instrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];

                config
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.WithProperty("ApplicationContext", serviceName)
                    .Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached) // Useful when doing Seq dashboards and want to remove logs under debugging session.
                     // Following configuration can also be specified in appsettings.json
                    .Enrich.WithSpan()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.Seq("http://seq:5341")
                    .WriteTo.ApplicationInsights(instrumentationKey, TelemetryConverter.Traces);
            });
        }
    }
}
