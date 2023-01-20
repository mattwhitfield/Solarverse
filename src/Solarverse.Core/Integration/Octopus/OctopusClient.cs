using Solarverse.Core.Helper;
using Solarverse.Core.Integration.Octopus.Models;
using Solarverse.Core.Models;
using Solarverse.Core.Models.Settings;
using System.Net.Http.Headers;
using System.Text;

namespace Solarverse.Core.Integration.Octopus
{
    public class OctopusClient : IEnergySupplierClient
    {
        private readonly HttpClient _httpClient;

        private readonly Dictionary<string, Product> _products = new Dictionary<string, Product>();
        private readonly Dictionary<string, string> _gridSupplyPointsByMpan = new Dictionary<string, string>();

        public OctopusClient(Configuration configuration)
        {
            _httpClient = new HttpClient();

            if (string.IsNullOrEmpty(configuration.ApiKeys?.Octopus))
            {
                throw new InvalidOperationException("Octopus API key was not configured");
            }

            var authenticationString = configuration.ApiKeys.Octopus + ":";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        }

        public async Task<IList<TariffRate>> GetTariffRates(string productCode, string mpan)
        {
            // workaround - bug in octopus API means you can't look up a GSP for
            // an outgoing MPAN
            string gspMpan = mpan;
            if (gspMpan == ConfigurationProvider.Configuration.OutgoingMeter?.MPAN)
            {
                gspMpan = ConfigurationProvider.Configuration.IncomingMeter?.MPAN ?? string.Empty;
            }

            var gsp = await GetGridSupplyPoint(gspMpan);
            if (string.IsNullOrWhiteSpace(gsp))
            {
                throw new InvalidOperationException($"Could not find grid supply point for the specified mpan '{mpan}'");
            }

            var ratesUri = await GetRatesUriForTariffAndGridSupplyPoint(productCode, gsp);
            var rates = await _httpClient.Get<AgileRates>(ratesUri);

            if (rates.Rates == null)
            {
                throw new InvalidDataException("Data returned from Octopus client is invalid");
            }

            return rates.Rates.Select(x => x.ToTariffRate()).ToList();
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
