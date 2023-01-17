using Solarverse.Core.Helper;
using Solarverse.Core.Models;

namespace Solarverse.Core.Data.CacheModels.Transforms
{
    public static class Transforms
    {
        public static HouseholdConsumption FromCache(this HouseholdConsumptionCache source)
        {
            return new HouseholdConsumption(
                true,
                source.ContainsInterpolatedPoints,
                source.DataPoints.Select(x => new HouseholdConsumptionDataPoint(x.Time, x.Consumption)));
        }

        public static HouseholdConsumptionCache ToCache(this HouseholdConsumption source)
        {
            var cache = new HouseholdConsumptionCache { ContainsInterpolatedPoints = source.ContainsInterpolatedPoints };
            source.DataPoints.Each(x => cache.DataPoints.Add(new HouseholdConsumptionDataPointCache { Time = x.Time, Consumption = x.Consumption }));
            return cache;
        }

        public static SolarForecast FromCache(this IEnumerable<SolarForecastPointCache> source)
        {
            var points = source.Select(x => new SolarForecastPoint(x.Time, x.PVEstimate));
            return new SolarForecast(true, points);
        }

        public static IList<SolarForecastPointCache> ToCache(this SolarForecast source)
        {
            return source.DataPoints.Select(x => new SolarForecastPointCache { PVEstimate = x.PVEstimate, Time = x.Time }).ToList();
        }

        public static IList<TariffRate> FromCache(this IEnumerable<TariffRateCache> source)
        {
            return source.Select(x => new TariffRate(x.Value, x.ValidFrom, x.ValidTo)).ToList();
        }

        public static IList<TariffRateCache> ToCache(this IEnumerable<TariffRate> source)
        {
            return source.Select(x => new TariffRateCache { Value = x.Value, ValidFrom = x.ValidFrom, ValidTo = x.ValidTo }).ToList();
        }
    }
}
