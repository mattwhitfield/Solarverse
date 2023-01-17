using Newtonsoft.Json;
using Solarverse.Core.Control;
using Solarverse.Core.Data.CacheModels;
using Solarverse.Core.Data.CacheModels.Transforms;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration;
using Solarverse.Core.Models;

namespace Solarverse.Core.Data
{
    public class FileDataStore : IDataStore
    {
        private readonly IIntegrationProvider _integrationProvider;
        private string _cacheRoot;

        public FileDataStore(IIntegrationProvider integrationProvider)
        {
            _cacheRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Solarverse",
                "Cache");
            _integrationProvider = integrationProvider;
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

            if (File.Exists(file))
            {
                var cached = JsonConvert.DeserializeObject<TCache>(File.ReadAllText(file));
                if (cached != null)
                {
                    return transformToData(cached);
                }
            }

            var data = await getData();
            if (shouldCache(data))
            {
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
                UpdatePeriods.ConsumptionUpdates,
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