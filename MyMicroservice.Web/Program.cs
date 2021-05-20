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
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .AddLogging(APP_NAME)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
