using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Diagnostics;

namespace MyMicroservice.Common.Extensions.Telemetry
{
    public static class Telemetry
    {
        public static void SafeRun(Func<IHost> host)
        {
            try
            {
                Activity.DefaultIdFormat = ActivityIdFormat.W3C;
                host().Run();
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
    }
}
