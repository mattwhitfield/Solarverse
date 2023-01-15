using System.Diagnostics;

namespace Solarverse.Core.Models
{
    [DebuggerDisplay("Time = {Time}, PVEstimate = {PVEstimate}")]

    public class SolarForecastPoint
    {
        public SolarForecastPoint(DateTime time, double pVEstimate)
        {
            Time = time;
            PVEstimate = pVEstimate;
        }

        public DateTime Time { get; }

        public double PVEstimate { get; }
    }

}
