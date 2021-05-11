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
                .AddSelfCheck()
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
