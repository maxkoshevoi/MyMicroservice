using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyMicroservice.Web
{
    public class WeatherClient
    {
        private readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly HttpClient client;
        private readonly ILogger<WeatherClient> _logger;

        private static readonly ActivitySource ActivitySource = new(nameof(WeatherClient));

        public WeatherClient(HttpClient client, ILogger<WeatherClient> logger)
        {
            this.client = client;
            this._logger = logger;
        }

        public async Task<WeatherForecast[]> GetWeatherAsync(string query)
        {

            _logger.LogInformation("{Method} - was called ", "WeatherClient.GetWeatherAsync()");

            try
            {
                using var activity = ActivitySource.StartActivity($"GET:", ActivityKind.Client);
                var responseMessage = await this.client.GetAsync("/weatherforecast?query=" + query);

                _logger.LogInformation("Was able to fetch data from backend!");

                if (responseMessage != null)
                {
                    var stream = await responseMessage.Content.ReadAsStreamAsync();
                    return await JsonSerializer.DeserializeAsync<WeatherForecast[]>(stream, options) ?? Array.Empty<WeatherForecast>();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            return Array.Empty<WeatherForecast>();
        }
    }
}
