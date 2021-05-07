using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using System;
using System.Diagnostics;
using System.Reflection;

namespace MyMicroservice
{
    public class Program
    {
        public static readonly string? APP_NAME = Assembly.GetExecutingAssembly().GetName().Name;

        public static void Main(string[] args)
        {
            try
            {
                Activity.DefaultIdFormat = ActivityIdFormat.W3C;
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                if (Log.Logger == null || Log.Logger.GetType().Name == "SilentLogger")
                {
                    // Loading configuration or Serilog failed.
                    InitializeDefaultLogger();
                }

                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAzureAppConfiguration)
                .UseSerilog((context, config) => ConfigureSerilog(context.Configuration, config))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        /// <summary>
        /// Creates a logger that can be captured by Azure logger.
        /// To enable Azure logger, in Azure Portal:
        /// 1. Go to WebApp
        /// 2. App Service logs
        /// 3. Enable "Application Logging (Filesystem)", "Application Logging (Filesystem)" and "Detailed error messages"
        /// 4. Set Retention Period (Days) to 10 or similar value
        /// 5. Save settings
        /// 6. Under Overview, restart web app
        /// 7. Go to Log Stream and observe the logs
        /// </summary>
        private static void InitializeDefaultLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
        }

        /// <summary>
        /// Adds configuration source linked to Azure App Configuration
        /// </summary>
        private static void ConfigureAzureAppConfiguration(IConfigurationBuilder configBuilder)
        {
            IConfiguration configuration = configBuilder.Build();
            if (configuration.UseFeatureManagement())
            {
                configBuilder.AddAzureAppConfiguration(options => options
                    .Connect(configuration["AppConfig:Endpoint"])
                    .UseFeatureFlags());
            }
        }

        /// <summary>
        /// See https://jkdev.me/asp-net-core-serilog for more information on how to configure Serilog
        /// </summary>
        private static void ConfigureSerilog(IConfiguration configuration, LoggerConfiguration config)
        {
            string? instrumentationKey = configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];

            config
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("ApplicationContext", APP_NAME)
                .Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached) // Useful when doing Seq dashboards and want to remove logs under debugging session.
                 // Following configuration can also be specified in appsettings.json
                .Enrich.WithSpan()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq("http://seq:5341")
                .WriteTo.ApplicationInsights(instrumentationKey, TelemetryConverter.Traces);
        }
    }
}
