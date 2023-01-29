namespace Solarverse.Core.Data
{
    public class RenderedTimeSeriesPoint
    {
        public RenderedTimeSeriesPoint(DateTime time, double value)
        {
            Time = time;
            Value = value;
        }

        public DateTime Time { get; }

        public double Value { get; }
    }
}