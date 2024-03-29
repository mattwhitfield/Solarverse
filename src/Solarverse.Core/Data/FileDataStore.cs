﻿using Microsoft.Extensions.Logging;
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
        private readonly ICurrentTimeProvider _currentTimeProvider;
        private string _cacheRoot;

        public FileDataStore(IIntegrationProvider integrationProvider, ICachePathProvider cachePathProvider, ILogger<FileDataStore> logger, ICurrentTimeProvider currentTimeProvider)
        {
            _cacheRoot = Path.Combine(
                cachePathProvider.CachePath,
                "Solarverse",
                "Cache");
            _integrationProvider = integrationProvider;
            _logger = logger;
            _currentTimeProvider = currentTimeProvider;
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
                _currentTimeProvider.UtcNow,
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
                x => x.Any() && x.Max(point => point.ValidFrom).Date > _currentTimeProvider.UtcNow.Date,
                UpdatePeriods.TariffUpdates,
                _currentTimeProvider.UtcNow,
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

        public void WriteTimeSeries(TimeSeries series)
        {
            var file = GetTimeSeriesFile();

            _logger.LogInformation($"Caching data");
            var cachable = new List<TimeSeriesPoint>(series.OrderBy(x => x.Time));
            File.WriteAllText(file, JsonConvert.SerializeObject(cachable));
        }

        public IList<TimeSeriesPoint>? ReadTimeSeries()
        {
            var file = GetTimeSeriesFile();

            if (File.Exists(file))
            {
                _logger.LogInformation($"File exists");

                var cached = JsonConvert.DeserializeObject<List<TimeSeriesPoint>>(File.ReadAllText(file));
                if (cached != null)
                {
                    _logger.LogInformation($"Deserialized successfully");
                    return cached;
                }
            }

            return null;
        }

        private string GetTimeSeriesFile()
        {
            var folder = GetCacheFolder("TimeSeries");
            var file = Path.Combine(folder, "current.json");
            _logger.LogInformation($"File path for {typeof(TimeSeries).GetFormattedName()} is {file}");
            return file;
        }
    }
}