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
        private readonly Configuration _configuration;

        public SolcastClient(Configuration configuration)
        {
            _httpClient = new HttpClient();

            if (string.IsNullOrEmpty(configuration.ApiKeys?.Solcast))
            {
                throw new InvalidOperationException("Solcast API key was not configured");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configuration.ApiKeys.Solcast);
            _configuration = configuration;
        }

        public async Task<SolarForecast> GetForecast()
        {
            var siteId = _configuration.SolcastSiteId;
            var forecastSet = await _httpClient.Get<ForecastSet>($"https://api.solcast.com.au/rooftop_sites/{siteId}/forecasts?format=json");

            return forecastSet.ToSolarForecast();
        }
    }
}
