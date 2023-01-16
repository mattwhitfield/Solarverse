using Solarverse.Core.Data;

namespace Solarverse
{
    public interface ITimeSeriesHandler
    {
        public void UpdateTimeSeries(TimeSeries series);
    }
}
