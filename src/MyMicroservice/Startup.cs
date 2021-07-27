using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMicroservice.Common.Extensions.FeatureManagement;
using MyMicroservice.Common.Extensions.HealthChecks;
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
            services.AddTracing(Program.APP_NAME);

            services.AddControllers();

            // Swagger
            services.AddSwaggerDocument(c => c.Title = Program.APP_NAME);

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
                app.UseOpenApi();
                app.UseSwaggerUi3(c =>
                {
                    c.DocExpansion = "list";
                    c.Path = string.Empty;
                });
            }

            app.UseLogging();
            app.UseAzureFeatureManagement();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFeatureManagement("/features");
                endpoints.MapHealthChecks();
            });
        }
    }

    static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            string redisConnectionStr = configuration.GetConnectionString("redis");

            services.AddHealthChecks()
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
