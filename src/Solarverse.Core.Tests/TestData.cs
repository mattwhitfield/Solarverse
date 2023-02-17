using Newtonsoft.Json;
using Solarverse.Core.Data.CacheModels;
using Solarverse.Core.Data.CacheModels.Transforms;
using Solarverse.Core.Models;
using System.Collections.Generic;
using System.Text;

namespace Solarverse.Core.Tests
{
    public static class TestData
    {
        private static HouseholdConsumption Household(byte[] source) => JsonConvert.DeserializeObject<HouseholdConsumptionCache>(Encoding.UTF8.GetString(source)).FromCache();

        public static HouseholdConsumption Household1 => Household(TestResources.HouseholdConsumption20230210);

        public static HouseholdConsumption Household2 => Household(TestResources.HouseholdConsumption20230211);

        public static HouseholdConsumption Household3 => Household(TestResources.HouseholdConsumption20230212);

        public static HouseholdConsumption Household4 => Household(TestResources.HouseholdConsumption20230213);

        public static SolarForecast SolarForecast => JsonConvert.DeserializeObject<IList<SolarForecastPointCache>>(Encoding.UTF8.GetString(TestResources.SolarForecast20230214)).FromCache();

        public static IList<TariffRate> IncomingRates => JsonConvert.DeserializeObject<IList<TariffRateCache>>(Encoding.UTF8.GetString(TestResources.IncomingRates20230213)).FromCache();

        public static IList<TariffRate> OutgoingRates => JsonConvert.DeserializeObject<IList<TariffRateCache>>(Encoding.UTF8.GetString(TestResources.OutgoingRates20230213)).FromCache();
    }
}
