using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MyMicroservice.Common.Extensions.FeatureManagement
{
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Adds key-value data from an Azure App Configuration store to a configuration builder.
        /// </summary>
        /// <param name="optional">Determines the behavior of the App Configuration provider when an exception occurs.
        ///  If false, the exception is thrown. If true, the exception is suppressed and no
        ///  settings are populated from Azure App Configuration.
        /// </param>
        public static IHostBuilder AddAzureFeatureManagement(this IHostBuilder hostBuilder, bool optional = false) => 
            hostBuilder.ConfigureAppConfiguration(configBuilder =>
            {
                IConfiguration configuration = configBuilder.Build();
                configBuilder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(configuration["AppConfig:Endpoint"]).UseFeatureFlags();
                }, optional);
            });
    }
}
