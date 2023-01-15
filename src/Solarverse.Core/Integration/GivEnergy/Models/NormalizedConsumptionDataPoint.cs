using System.Diagnostics;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    [DebuggerDisplay("Time = {Time}, Consumption = {Consumption}")]
    public class NormalizedConsumptionDataPoint
    {
        public NormalizedConsumptionDataPoint(DateTime time, double consumption)
        {
            Time = time;
            Consumption = consumption;
        }

        public DateTime Time { get; }

        public double Consumption { get; }
    }
}
