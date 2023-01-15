using System.Diagnostics;

namespace Solarverse.Core.Models
{
    [DebuggerDisplay("Time = {Time}, Consumption = {Consumption}")]
    public class HouseholdConsumptionDataPoint
    {
        public HouseholdConsumptionDataPoint(DateTime time, double consumption)
        {
            Time = time;
            Consumption = consumption;
        }

        public DateTime Time { get; }

        public double Consumption { get; }
    }
}
