using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMicroservice.Common.Extensions.Telemetry;
using System.Threading.Tasks;

namespace HealthCheckUI
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
            services.AddTracing(Program.APP_NAME);
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(10); // Configures the UI to poll for health checks updates every 10 seconds
                setup.AddHealthCheckEndpoint("MyMicroservice", $"http://mymicroservice:80/health/startup");
                setup.AddHealthCheckEndpoint("MyMicroservice.Web", $"http://mymicroservice.web:80/health/startup");
            }).AddInMemoryStorage();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseHealthChecksUI(config =>
            {
                config.UIPath = "/healthchecks-ui";
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", context =>
                {
                    context.Response.Redirect($"/healthchecks-ui");
                    return Task.CompletedTask;
                });
            });
        }
    }
}
