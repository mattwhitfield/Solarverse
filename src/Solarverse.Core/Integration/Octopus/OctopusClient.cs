using Solarverse.Core.Helper;
using Solarverse.Core.Integration.Octopus.Models;
using System.Net.Http.Headers;
using System.Text;

namespace Solarverse.Core.Integration.Octopus
{
    public class OctopusClient : IOctopusClient
    {
        private readonly HttpClient _httpClient;

        private readonly Dictionary<string, Product> _products = new Dictionary<string, Product>();
        private readonly Dictionary<string, string> _gridSupplyPointsByMpan = new Dictionary<string, string>();

        public OctopusClient(string key)
        {
            _httpClient = new HttpClient();

            var authenticationString = key + ":";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        }

        public async Task<AgileRates> GetAgileRates(string productCode, string mpan)
        {
            var gsp = await GetGridSupplyPoint(mpan);
            if (string.IsNullOrWhiteSpace(gsp))
            {
                throw new InvalidOperationException($"Could not find grid supply point for the specified mpan '{mpan}'");
            }

            var ratesUri = await GetRatesUriForTariffAndGridSupplyPoint(productCode, gsp);
            return await _httpClient.Get<AgileRates>(ratesUri);
        }

        private async Task<string> GetRatesUriForTariffAndGridSupplyPoint(string productCode, string gridSupplyPoint)
        {
            if (!_products.TryGetValue(productCode, out var product))
            {
                product = await _httpClient.Get<Product>($"https://api.octopus.energy/v1/products/{productCode}/");

                if (product?.TariffTypes == null)
                {
                    throw new InvalidOperationException($"Could not find details for the specified product '{productCode}'");
                }

                _products[productCode] = product;
            }

            if (product.TariffTypes == null || !product.TariffTypes.TryGetValue(gridSupplyPoint, out var tariffType))
            {
                throw new InvalidOperationException($"Could not find tariff for the specified product '{productCode}' and grid supply point '{gridSupplyPoint}'");
            }

            if (tariffType.DirectDebitMonthly == null)
            {
                throw new InvalidOperationException($"Could not find direct debit monthly rates for the specified product '{productCode}' and grid supply point '{gridSupplyPoint}'");
            }

            var gspTariffCode = tariffType.DirectDebitMonthly.Code;

            return $"https://api.octopus.energy/v1/products/{productCode}/electricity-tariffs/{gspTariffCode}/standard-unit-rates/";
        }

        private async Task<string?> GetGridSupplyPoint(string mpan)
        {
            if (!_gridSupplyPointsByMpan.TryGetValue(mpan, out var gridSupplyPoint))
            {
                gridSupplyPoint = (await _httpClient.Get<MeterPoint>($"https://api.octopus.energy/v1/electricity-meter-points/{mpan}/"))?.GridSupplyPoint;
                if (!string.IsNullOrWhiteSpace(gridSupplyPoint))
                {
                    _gridSupplyPointsByMpan[mpan] = gridSupplyPoint;
                }
            }

            return gridSupplyPoint;
        }
    }
}
