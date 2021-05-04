using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
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

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<string> Get([FromServices] IDistributedCache cache)
        {
            var weather = await cache.GetStringAsync("weather");
            if (weather == null)
            {
                _logger.LogInformation("Weather data cache miss");

                weather = GetWeatherStaticData();
                await cache.SetStringAsync("weather", weather, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5)
                });
            }
            else
            {
                _logger.LogInformation("Weather data found in cache");
            }

            _logger.LogInformation("Weather data - {data}", weather);

            return weather;
        }

        internal static string GetWeatherStaticData()
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
