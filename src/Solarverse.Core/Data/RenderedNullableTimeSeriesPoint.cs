namespace Solarverse.Core.Data
{
    public class RenderedNullableTimeSeriesPoint
    {
        public RenderedNullableTimeSeriesPoint(DateTime time, double? value)
        {
            Time = time;
            Value = value;
        }

        public DateTime Time { get; }

        public double? Value { get; }
    }
}