using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace MyMicroservice
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
         
            // Swagger
            services.AddSwaggerGen();

            // Redis cache
            services.AddStackExchangeRedisCache(o =>
            {
                string con = Configuration.GetConnectionString("redis");
                o.Configuration = con;
            });

            // Feature management
            // See https://github.com/microsoft/FeatureManagement-Dotnet#aspnet-core-feature-flags for more info
            if (Configuration.UseFeatureManagement())
            {
                services.AddFeatureManagement();
                services.AddAzureAppConfiguration();
            }

            // Health checks
            services.AddHealthChecks(Configuration);

            // Logging and tracing
            services.AddOpenTelemetryTracing(Configuration);
            services.AddAppInsights(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyMicroservice v1"));
            }

            // Refresh feature flags
            if (Configuration.UseFeatureManagement())
            {
                app.UseAzureAppConfiguration();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/readiness", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name == "self"
                });
            });
        }
    }

    static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            string redisConnectionStr = configuration.GetConnectionString("redis");

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddRedis(redisConnectionStr, name: "redis-check", tags: new[] { "redis" });

            return services;
        }

        public static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services, IConfiguration configuration)
        {
            var exporter = configuration.GetValue<string>("UseExporter").ToLowerInvariant();
            services.AddOpenTelemetryTracing(builder =>
            {
                if (exporter == "zipkin")
                {
                    builder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Program.APP_NAME))
                        .AddZipkinExporter(zipkinOptions =>
                        {
                            zipkinOptions.Endpoint = new Uri($"http://zipkin:9411/api/v2/spans");
                        });
                }
                else
                {
                    builder.AddConsoleExporter();
                }

                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

            return services;
        }

        public static IServiceCollection AddAppInsights(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsTelemetry(configuration);
            services.AddApplicationInsightsKubernetesEnricher();

            return services;
        }

        public static bool UseFeatureManagement(this IConfiguration configuration) =>
            configuration.GetValue("UseFeatureManagement", false) == true;
    }
}
