using Microsoft.Extensions.Logging;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration.Solcast.Models;
using Solarverse.Core.Models;
using Solarverse.Core.Models.Settings;
using System.Net.Http.Headers;

namespace Solarverse.Core.Integration.Solcast
{
    public class SolcastClient : ISolarForecastClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ILogger<SolcastClient> _logger;

        public SolcastClient(IConfigurationProvider configurationProvider, ILogger<SolcastClient> logger)
        {
            _httpClient = new HttpClient();

            if (string.IsNullOrEmpty(configurationProvider.Configuration.ApiKeys?.Solcast))
            {
                throw new InvalidOperationException("Solcast API key was not configured");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configurationProvider.Configuration.ApiKeys.Solcast);
            _configurationProvider = configurationProvider;
            _logger = logger;
        }

        public async Task<SolarForecast> GetForecast()
        {
            var siteId = _configurationProvider.Configuration.SolcastSiteId;
            var forecastSet = await _httpClient.Get<ForecastSet>(_logger, $"https://api.solcast.com.au/rooftop_sites/{siteId}/forecasts?format=json");

            return forecastSet.ToSolarForecast();
        }
    }
}
