using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MyMicroservice.Common.Extensions.Telemetry;
using System.Reflection;

namespace MyMicroservice.Web
{
    public class Program
    {
        public static readonly string? APP_NAME = Assembly.GetExecutingAssembly().GetName().Name;

        public static void Main(string[] args)
        {
            Telemetry.SafeRun(() => CreateHostBuilder(args).Build());
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .AddTelemetry(APP_NAME)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
