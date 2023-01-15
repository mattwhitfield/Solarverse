using Solarverse.Core.Models;

namespace Solarverse.Core.Integration
{
    public interface ISolarForecastClient
    {
        Task<SolarForecast> GetForecast();
    }
}