using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace MyMicroservice.Common.Extensions.Telemetry
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds AppInsights and OpenTelemetry tracing<br/>
        /// (needs to be before AddControllers)
        /// </summary>
        public static IServiceCollection AddTelemetry(this IServiceCollection services, string? serviceName)
        {
            services.AddAppInsights(serviceName);
            services.AddOpenTelemetryTracing(serviceName);

            return services;
        }

        static IServiceCollection AddAppInsights(this IServiceCollection services, string? serviceName)
        {
            services.AddSingleton<ITelemetryInitializer>(new MyTelemetryInitializer(serviceName));
            services.AddApplicationInsightsTelemetry();
            //services.AddApplicationInsightsKubernetesEnricher();

            return services;
        }

        static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services, string? serviceName)
        {
            services.AddOpenTelemetryTracing(builder =>
            {
                builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddZipkinExporter(zipkinOptions =>
                    {
                        zipkinOptions.Endpoint = new Uri($"http://zipkin:9411/api/v2/spans");
                    })
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

            return services;
        }
    }
}
