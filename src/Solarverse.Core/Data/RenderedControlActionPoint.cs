namespace Solarverse.Core.Data
{
    public class RenderedControlActionPoint
    {
        public RenderedControlActionPoint(DateTime time, ControlAction value)
        {
            Time = time;
            Value = value;
        }

        public DateTime Time { get; }

        public ControlAction Value { get; }
    }
}
