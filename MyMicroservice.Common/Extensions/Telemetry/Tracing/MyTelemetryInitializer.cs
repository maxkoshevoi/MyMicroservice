using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace MyMicroservice.Common.Extensions.Telemetry
{
    /// <summary>
    /// Adds custom role (service) name to the telemetry<br/>
    /// See <see href="https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-map?tabs=net#understanding-cloud-role-name-within-the-context-of-the-application-map">docs.microsoft.com/azure/azure-monitor/app/app-map</see> for more info
    /// </summary>
    class MyTelemetryInitializer : ITelemetryInitializer
    {
        readonly string? roleName;

        public MyTelemetryInitializer(string? roleName)
        {
            this.roleName = roleName;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
            {
                telemetry.Context.Cloud.RoleName = roleName;
            }
        }
    }
}
