using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyMicroservice.Common.Extensions.HealthChecks
{
    public static class HealthChecksBuilderExtensions
    {
        /// <summary>
        /// Adds "self" health check used for "liveness" endpoint
        /// </summary>
        public static IHealthChecksBuilder AddSelfCheck(this IHealthChecksBuilder builder) =>
            builder.AddCheck("self", () => HealthCheckResult.Healthy());
    }
}
