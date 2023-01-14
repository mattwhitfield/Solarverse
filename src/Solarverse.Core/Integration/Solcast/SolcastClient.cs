using Solarverse.Core.Helper;
using Solarverse.Core.Integration.Solcast.Models;
using System.Net.Http.Headers;

namespace Solarverse.Core.Integration.Solcast
{
    public class SolcastClient
    {
        private readonly HttpClient _httpClient;


        public SolcastClient(string key)
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
        }

        public Task<ForecastSet> GetForecastSet(string siteId)
        {
            return _httpClient.Get<ForecastSet>($"https://api.solcast.com.au/rooftop_sites/{siteId}/forecasts?format=json");
        }
    }
}
