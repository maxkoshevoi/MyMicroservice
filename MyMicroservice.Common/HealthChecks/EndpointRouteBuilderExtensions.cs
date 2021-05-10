using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace MyMicroservice.Common.HealthChecks
{
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Maps "liveness" and "readiness" health checks 
        /// </summary>
        public static IEndpointRouteBuilder MapHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            // Is container alive
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name == "self"
            });

            // Is container able to perform work (all dependencies are also checked)
            endpoints.MapHealthChecks("/readiness", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            return endpoints;
        }
    }
}
