using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace MyMicroservice.Common.Extensions.FeatureManagement
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds feature management and Azure App Configuration services<br/>
        /// See <see href="https://docs.microsoft.com/en-us/azure/azure-app-configuration/use-feature-flags-dotnet-core#feature-flag-checks"/> for more info
        /// </summary>
        public static IServiceCollection AddAzureFeatureManagement(this IServiceCollection services)
        {
            services.AddFeatureManagement();
            services.AddAzureAppConfiguration();

            return services;
        }
    }
}
