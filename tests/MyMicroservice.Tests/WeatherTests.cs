using MyMicroservice.Controllers;
using Xunit;

namespace MyMicroservice.Tests
{
    public class WeatherTests
    { 
        [Fact]
        public void Test1()
        {
            string staticWeather = WeatherForecastController.GetWeatherData();
            
            Assert.NotNull(staticWeather);
        }
    }
}
