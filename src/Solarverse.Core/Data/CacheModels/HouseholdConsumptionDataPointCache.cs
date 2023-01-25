using System.Diagnostics;

namespace Solarverse.Core.Data.CacheModels
{
    [DebuggerDisplay("Time = {Time}, Consumption = {Consumption}, Solar = {Solar}, Import = {Import}, Export = {Export}, Charge = {Charge}, Discharge = {Discharge}, BatteryPercentage = {BatteryPercentage}")]
    public class HouseholdConsumptionDataPointCache
    {
        public DateTime Time { get; set; }

        public double Consumption { get; set; }

        public double Solar { get; set; }

        public double Import { get; set; }

        public double Export { get; set; }

        public double Charge { get; set; }

        public double Discharge { get; set; }

        public double BatteryPercentage { get; set; }
    }
}
