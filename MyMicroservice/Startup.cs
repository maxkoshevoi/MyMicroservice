using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

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
            services.AddAppInsights(Configuration);
            services.AddControllers();
            services.AddSwaggerGen();
            services.AddStackExchangeRedisCache(o =>
            {
                string con = Configuration.GetConnectionString("redis");
                o.Configuration = con;
            });

            services.AddHealthChecks(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyMicroservice v1"));
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

        public static IServiceCollection AddAppInsights(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsTelemetry(configuration);
            services.AddApplicationInsightsKubernetesEnricher();

            return services;
        }
    }
}
