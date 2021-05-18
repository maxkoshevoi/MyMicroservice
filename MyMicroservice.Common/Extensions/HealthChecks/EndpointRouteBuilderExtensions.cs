using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace MyMicroservice.Common.Extensions.HealthChecks
{
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Maps "liveness" and "startup" health checks.<br/>
        /// - liveness - only checks that app can serve HTTP requests.<br/>
        /// This is for detecting whether the application process has crashed/deadlocked. If a liveness probe fails, Kubernetes will stop the pod, and create a new one.<br/>
        /// - startup - checks all dependencies (primarily to catch configuration errors).<br/>
        /// This is used when the container starts up, to indicate that it's ready. Once the startup probe succeeds, Kubernetes switches to using the liveness probe to determine if the application is alive<br/>
        /// <br/>
        /// See <see href="https://andrewlock.net/deploying-asp-net-core-applications-to-kubernetes-part-6-adding-health-checks-with-liveness-readiness-and-startup-probes"/> for more information
        /// </summary>
        public static IEndpointRouteBuilder MapHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            // Is container alive
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = _ => false
            });

            // Is container able to perform work (all dependencies are also checked)
            endpoints.MapHealthChecks("/startup", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            return endpoints;
        }
    }
}
