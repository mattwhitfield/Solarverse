using System.Diagnostics;

namespace Solarverse.Core.Data.CacheModels
{
    [DebuggerDisplay("Time = {Time}, PVEstimate = {PVEstimate}")]

    public class SolarForecastPointCache
    {
        public DateTime Time { get; set; }

        public double PVEstimate { get; set; }
    }

}
