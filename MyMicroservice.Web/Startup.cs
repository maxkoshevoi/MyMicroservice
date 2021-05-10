using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMicroservice.Common.Extensions.HealthChecks;
using MyMicroservice.Common.Extensions.Telemetry;
using System;

namespace MyMicroservice.Web
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
            services.AddTelemetry(Program.APP_NAME);
            services.AddRazorPages();
            services.AddHttpClient<WeatherClient>(client =>
            {
                client.BaseAddress = new Uri("http://mymicroservice:80");
            });
            services.AddHealthChecks(Configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseTelemetry();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks();
            });
        }
    }

    static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddSelfCheck()
                .AddUrlGroup(new Uri($"http://mymicroservice:80/readiness"), name: "backendapi-check", tags: new string[] { "backendapi" });

            return services;
        }
    }
}
