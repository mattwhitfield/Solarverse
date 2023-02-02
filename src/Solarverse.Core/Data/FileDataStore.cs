using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Solarverse.Core.Control;
using Solarverse.Core.Data.CacheModels.Transforms;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration;
using Solarverse.Core.Models;

namespace Solarverse.Core.Data
{
    public class FileDataStore : IDataStore
    {
        private readonly IIntegrationProvider _integrationProvider;
        private readonly ILogger<FileDataStore> _logger;
        private string _cacheRoot;

        public FileDataStore(IIntegrationProvider integrationProvider, ILogger<FileDataStore> logger)
        {
            _cacheRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Solarverse",
                "Cache");
            _integrationProvider = integrationProvider;
            _logger = logger;
        }

        private async Task<TData> Get<TData, TCache>(
            Func<Task<TData>> getData,
            Func<TData, TCache> transformToCache,
            Func<TCache, TData> transformToData,
            Func<TData, bool> shouldCache,
            Period dataPeriod,
            DateTime dateTime,
            string cacheCategory)
        {
            var folder = GetCacheFolder(cacheCategory);
            var date = dataPeriod.GetLast(dateTime);
            var file = Path.Combine(folder, date.ToString("yyyyMMdd-HHmmss") + ".json");

            _logger.LogInformation($"File path for {typeof(TData).GetFormattedName()} at {dateTime} is {file}");

            if (File.Exists(file))
            {
                _logger.LogInformation($"File exists");

                var cached = JsonConvert.DeserializeObject<TCache>(File.ReadAllText(file));
                if (cached != null)
                {
                    _logger.LogInformation($"Deserialized successfully");
                    return transformToData(cached);
                }
            }

            _logger.LogInformation($"Getting data from source");
            var data = await getData();
            if (shouldCache(data))
            {
                _logger.LogInformation($"Caching data");
                File.WriteAllText(file, JsonConvert.SerializeObject(transformToCache(data)));
            }

            return data;
        }

        public Task<HouseholdConsumption> GetHouseholdConsumptionFor(DateTime date)
        {
            return Get(
                () => _integrationProvider.InverterClient.GetHouseholdConsumptionFor(date),
                x => x.ToCache(),
                x => x.FromCache(),
                x => x.IsValid,
                UpdatePeriods.ConsumptionCacheUpdates,
                date,
                "Household");
        }

        public Task<SolarForecast> GetSolarForecast()
        {
            return Get(
                () => _integrationProvider.SolarForecastClient.GetForecast(),
                x => x.ToCache(),
                x => x.FromCache(),
                x => x.IsValid,
                UpdatePeriods.SolarForecastUpdates,
                DateTime.UtcNow,
                "SolarForecasts");
        }

        public async Task<IList<TariffRate>?> GetTariffRates(string productCode, string mpan)
        {
            if (string.IsNullOrWhiteSpace(productCode) ||
                string.IsNullOrWhiteSpace(mpan))
            {
                return null;
            }

            return await Get(
                () => _integrationProvider.EnergySupplierClient.GetTariffRates(productCode, mpan),
                x => x.ToCache(),
                x => x.FromCache(),
                _ => true,
                UpdatePeriods.TariffUpdates,
                DateTime.UtcNow,
                "Tariff-" + mpan);
        }

        private string GetCacheFolder(string @for)
        {
            var folder = Path.Combine(_cacheRoot, @for);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }
    }
}