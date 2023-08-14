using Microsoft.Extensions.Logging;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration.ForecastSolar.Models;
using Solarverse.Core.Models;

namespace Solarverse.Core.Integration.ForecastSolar
{
    public class ForecastSolarClient : ISolarForecastClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ILogger<ForecastSolarClient> _logger;

        public ForecastSolarClient(IConfigurationProvider configurationProvider, ILogger<ForecastSolarClient> logger)
        {
            _httpClient = new HttpClient();
            _configurationProvider = configurationProvider;
            _logger = logger;
        }

        public async Task<SolarForecast> GetForecast()
        {
            // todo - rename the API key field to 'solar provider'
            var apiKey = _configurationProvider.Configuration.ApiKeys.Solcast;
            if (!string.IsNullOrEmpty(apiKey))
            {
                apiKey += "/";
            }

            var uri = $"https://api.forecast.solar/{apiKey}estimate/watts/50.933651/-1.301568/45/30/5?time=utc";
            var forecastSet = await _httpClient.Get<Forecast>(_logger, uri);

            return forecastSet.ToSolarForecast();
        }
    }
}
