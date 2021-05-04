using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.FeatureManagement;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyMicroservice.Middlewares
{
    public class FeatureManagementMiddleware
    {
        private static readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public FeatureManagementMiddleware(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context, IFeatureManager featureManager)
        {
            List<EvaluationResponse> evaluationsResponse = new();
            StringValues featureNames = context.Request.Query["featureName"];

            foreach (var featureName in featureNames)
            {
                var isEnabled = await featureManager.IsEnabledAsync(featureName);
                evaluationsResponse.Add(new EvaluationResponse(featureName, isEnabled));
            }

            await WriteResponse(context, evaluationsResponse);
        }

        private static Task WriteResponse(HttpContext currentContext, IEnumerable<EvaluationResponse> response) =>
            WriteAsync(
                currentContext,
                JsonSerializer.Serialize(response, options: _serializerOptions),
                MediaTypeNames.Application.Json,
                StatusCodes.Status200OK);

        private static async Task WriteAsync(
           HttpContext context,
           string content,
           string contentType,
           int statusCode)
        {
            context.Response.Headers["Content-Type"] = new[] { contentType };
            context.Response.Headers["Cache-Control"] = new[] { "no-cache, no-store, must-revalidate" };
            context.Response.Headers["Pragma"] = new[] { "no-cache" };
            context.Response.Headers["Expires"] = new[] { "0" };
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(content);
        }

        private record EvaluationResponse(string Name, bool Enabled);
    }
}
