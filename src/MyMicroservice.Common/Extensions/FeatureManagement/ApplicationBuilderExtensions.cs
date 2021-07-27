using Microsoft.AspNetCore.Builder;

namespace MyMicroservice.Common.Extensions.FeatureManagement
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Refreshes feature flags
        /// </summary>
        public static IApplicationBuilder UseAzureFeatureManagement(this IApplicationBuilder app) =>
            app.UseAzureAppConfiguration();
    }
}
