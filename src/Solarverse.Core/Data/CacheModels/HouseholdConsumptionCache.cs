namespace Solarverse.Core.Data.CacheModels
{
    public class HouseholdConsumptionCache
    {
        public bool ContainsInterpolatedPoints { get; set; }

        public List<HouseholdConsumptionDataPointCache> DataPoints { get; }
            = new List<HouseholdConsumptionDataPointCache>();
    }
}
