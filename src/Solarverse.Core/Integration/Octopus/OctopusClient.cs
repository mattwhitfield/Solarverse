﻿using Microsoft.Extensions.Logging;
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
        private readonly ILogger<OctopusClient> _logger;
        private readonly IConfigurationProvider _configurationProvider;

        public OctopusClient(ILogger<OctopusClient> logger, IConfigurationProvider configurationProvider)
        {
            _httpClient = new HttpClient();

            if (string.IsNullOrEmpty(configurationProvider.Configuration.ApiKeys?.Octopus))
            {
                throw new InvalidOperationException("Octopus API key was not configured");
            }

            var authenticationString = configurationProvider.Configuration.ApiKeys.Octopus + ":";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            _logger = logger;
            _configurationProvider = configurationProvider;
        }

        public async Task<IList<TariffRate>> GetTariffRates(string productCode, string mpan)
        {
            // workaround - bug in octopus API means you can't look up a GSP for
            // an outgoing MPAN
            string gspMpan = mpan;
            if (gspMpan == _configurationProvider.Configuration.OutgoingMeter?.MPAN)
            {
                gspMpan = _configurationProvider.Configuration.IncomingMeter?.MPAN ?? string.Empty;
            }

            var gsp = await GetGridSupplyPoint(gspMpan);
            if (string.IsNullOrWhiteSpace(gsp))
            {
                throw new InvalidOperationException($"Could not find grid supply point for the specified mpan '{mpan}'");
            }

            var ratesUri = await GetRatesUriForTariffAndGridSupplyPoint(productCode, gsp);
            var rates = await _httpClient.Get<AgileRates>(_logger, ratesUri);

            if (rates.Rates == null)
            {
                throw new InvalidDataException("Data returned from Octopus client is invalid");
            }

            var rawRates = rates.Rates.Select(x => x.ToTariffRate()).Where(x => x.ValidFrom.Date >= DateTime.UtcNow.Date.AddDays(-1)).OrderBy(x => x.ValidFrom).ToList();
            var processedRates = new List<TariffRate>();

            foreach (var rawRate in rawRates)
            {
                if (rawRate.ValidFrom > DateTime.UtcNow.AddDays(3))
                {
                    break;
                }

                if (rawRate.ValidTo > rawRate.ValidFrom.AddMinutes(30))
                {
                    var current = rawRate.ValidFrom;
                    while (current < rawRate.ValidTo)
                    {
                        processedRates.Add(new TariffRate(rawRate.Value, current, current.AddMinutes(30)));
                        current = current.AddMinutes(30);
                    }
                }
                else
                {
                    processedRates.Add(rawRate);
                }
            }
            
            return processedRates;
        }

        private async Task<string> GetRatesUriForTariffAndGridSupplyPoint(string productCode, string gridSupplyPoint)
        {
            if (!_products.TryGetValue(productCode, out var product))
            {
                product = await _httpClient.Get<Product>(_logger, $"https://api.octopus.energy/v1/products/{productCode}/");

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
                gridSupplyPoint = (await _httpClient.Get<MeterPoint>(_logger, $"https://api.octopus.energy/v1/electricity-meter-points/{mpan}/"))?.GridSupplyPoint;
                if (!string.IsNullOrWhiteSpace(gridSupplyPoint))
                {
                    _gridSupplyPointsByMpan[mpan] = gridSupplyPoint;
                }
            }

            return gridSupplyPoint;
        }
    }
}
