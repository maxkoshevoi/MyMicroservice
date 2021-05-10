using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyMicroservice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IFeatureManager _featureManager;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IFeatureManager featureManager)
        {
            _logger = logger;
            _featureManager = featureManager;
        }

        [HttpGet]
        public async Task<string> Get(string query, [FromServices] IDistributedCache cache)
        {
            bool useCache = await _featureManager.IsEnabledAsync("RedisCache");
            string weather = useCache ? await GetCachedWeatherData(cache) : GetWeatherData();

            _logger.LogInformation("Weather data - {data}", weather);

            return weather;
        }

        private async Task<string> GetCachedWeatherData(IDistributedCache cache)
        {
            string? weather = await cache.GetStringAsync("weather");
            if (weather == null)
            {
                _logger.LogInformation("Weather data cache miss");

                weather = GetWeatherData();
                await cache.SetStringAsync("weather", weather, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                });
            }
            else
            {
                _logger.LogInformation("Weather data found in cache");
            }

            return weather;
        }

        internal static string GetWeatherData()
        {
            Random rng = new();
            var forecasts = Enumerable.Range(1, 5)
                .Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                });

            return JsonSerializer.Serialize(forecasts);
        }
    }
}
