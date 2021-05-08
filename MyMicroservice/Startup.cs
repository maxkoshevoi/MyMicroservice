using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using MyMicroservice.Common.Extensions.FeatureManagement;
using MyMicroservice.Common.Extensions.Telemetry;
using MyMicroservice.Middlewares;

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
            // Logging and tracing
            services.AddTelemetry(Program.APP_NAME);

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
            services.AddAzureFeatureManagement();

            // Health checks
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

            app.UseTelemetry();
            app.UseAzureAppConfiguration();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFeatureManagement("/features");
                
                // Is container alive
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name == "self"
                });

                // Is container able to perform work (all dependencies are also checked)
                endpoints.MapHealthChecks("/readiness", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
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
    }

    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapFeatureManagement(this IEndpointRouteBuilder endpoints, string pattern)
        {
            var pipeline = endpoints.CreateApplicationBuilder()
                .UseMiddleware<FeatureManagementMiddleware>()
                .Build();

            return endpoints.MapGet(pattern, pipeline);
        }
    }
}
