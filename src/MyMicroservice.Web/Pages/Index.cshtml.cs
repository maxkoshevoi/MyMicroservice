using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MyMicroservice.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public WeatherForecast[] Forecasts { get; set; } = Array.Empty<WeatherForecast>();

        public string ErrorMessage { get; set; } = string.Empty;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet([FromServices] WeatherClient client)
        {
            string query = "12.9716,77.5946";
            if (!string.IsNullOrEmpty(Request.Query["query"]))
            {
                query = Request.Query["query"];
            }

            Console.WriteLine($"query -- {query}");

            Forecasts = await client.GetWeatherAsync(query);
            if (Forecasts.Length == 0)
            {
                ErrorMessage = "We are unable to fetch weather info right now. Please try again after some time.";
            }
        }
    }
}
