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
        static readonly string? APP_NAME = Assembly.GetExecutingAssembly().GetName().Name;

        public static void Main(string[] args)
        {
            Log.Logger = CreateSerilogLogger();

            try
            {
                Log.Information("Starting Web Host");

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
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static Serilog.ILogger CreateSerilogLogger()
        {
            string? instrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");

            return new LoggerConfiguration()
                .Enrich.WithProperty("ApplicationContext", APP_NAME)
                .Enrich.WithSpan()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq("http://seq:5341")
                .WriteTo.ApplicationInsights(instrumentationKey, TelemetryConverter.Traces)
                .CreateLogger();
        }
    }
}
