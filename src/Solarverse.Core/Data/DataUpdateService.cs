using Solarverse.Core.Models;

namespace Solarverse.Core.Data
{
    public class CurrentDataService : ICurrentDataService
    {
        public TimeSeries TimeSeries { get; } = new TimeSeries();

        public InverterCurrentState CurrentState { get; private set; } = InverterCurrentState.Default;

        public event EventHandler<EventArgs>? TimeSeriesUpdated;

        public event EventHandler<EventArgs>? CurrentStateUpdated;

        public void Cull(TimeSpan deleteOlderThan)
        {
            var pointsRemoved = TimeSeries.Cull(deleteOlderThan);
            if (pointsRemoved)
            {
                TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Update(InverterCurrentState currentState)
        {
            CurrentState = currentState;
            CurrentStateUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Update(SolarForecast forecast)
        {
            if (forecast.IsValid)
            {
                TimeSeries.AddPointsFrom(forecast.DataPoints, x => x.Time, x => x.PVEstimate, (val, pt) => pt.PVForecastKwh = val);
                TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateIncomingRates(IList<TariffRate> incomingRates)
        {
            TimeSeries.AddPointsFrom(incomingRates, x => x.ValidFrom, x => x.Value, (val, pt) => pt.IncomingRate = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateOutgoingRates(IList<TariffRate> outgoingRates)
        {
            TimeSeries.AddPointsFrom(outgoingRates, x => x.ValidFrom, x => x.Value, (val, pt) => pt.OutgoingRate = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
