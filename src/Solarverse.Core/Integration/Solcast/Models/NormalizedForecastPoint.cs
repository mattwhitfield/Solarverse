namespace Solarverse.Core.Integration.Solcast.Models
{
    public class NormalizedForecastPoint
    {
        public NormalizedForecastPoint(DateTime time, double pVEstimate)
        {
            Time = time;
            PVEstimate = pVEstimate;
        }

        public DateTime Time { get; }

        public double PVEstimate { get; }
    }

}
