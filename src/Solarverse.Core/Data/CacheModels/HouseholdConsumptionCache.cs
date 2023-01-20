using System.Diagnostics;

namespace Solarverse.Core.Data.CacheModels
{
    [DebuggerDisplay("DataPoints: {DataPoints.Count}")]
    public class HouseholdConsumptionCache
    {
        public bool ContainsInterpolatedPoints { get; set; }

        public List<HouseholdConsumptionDataPointCache> DataPoints { get; }
            = new List<HouseholdConsumptionDataPointCache>();
    }
}
