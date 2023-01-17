using System.Diagnostics;

namespace Solarverse.Core.Data.CacheModels
{
    [DebuggerDisplay("Time = {Time}, Consumption = {Consumption}")]
    public class HouseholdConsumptionDataPointCache
    {
        public DateTime Time { get; set; }

        public double Consumption { get; set; }
    }
}
