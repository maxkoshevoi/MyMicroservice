using Microsoft.AspNetCore.Hosting;
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
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, config) =>
                {
                    // See https://jkdev.me/asp-net-core-serilog for more information on how to configure Serilog

                    string? instrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];

                    config
                        .ReadFrom.Configuration(context.Configuration)
                        .Enrich.WithProperty("ApplicationContext", APP_NAME)
                        .Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached) // Useful when doing Seq dashboards and want to remove logs under debugging session.
                        .Enrich.WithSpan()
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.Seq("http://seq:5341")
                        .WriteTo.ApplicationInsights(instrumentationKey, TelemetryConverter.Traces);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
