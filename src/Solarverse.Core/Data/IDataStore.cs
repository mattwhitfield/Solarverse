using Solarverse.Core.Models;

namespace Solarverse.Core.Data
{
    public interface IDataStore
    {
        public Task<HouseholdConsumption> GetHouseholdConsumptionFor(DateTime date);

        public Task<IList<TariffRate>?> GetTariffRates(string productCode, string mpan);

        public Task<SolarForecast> GetSolarForecast();

        public void WriteTimeSeries(TimeSeries series);

        public IList<TimeSeriesPoint>? ReadTimeSeries();
    }
}
