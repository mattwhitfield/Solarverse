using Solarverse.Core.Helper;
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

        public void Update(PredictedConsumption consumption)
        {
            if (!consumption.DataPoints.Any())
            {
                return;
            }

            // a little hack to make the first 'predicted' consumption point line up with the 'actual' graph
            var min = consumption.DataPoints.Min(x => x.Time);
            if (TimeSeries.TryGetDataPointFor(min, out var point) && point != null)
            {
                consumption.DataPoints.Where(x => x.Time == min).Each(x => x.Consumption = point.ActualConsumptionKwh.GetValueOrDefault(x.Consumption));
            }

            TimeSeries.Set(x => x.ConsumptionForecastKwh = null);
            TimeSeries.AddPointsFrom(consumption.DataPoints, x => x.Time, x => x.Consumption, (val, pt) => pt.ConsumptionForecastKwh = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Update(HouseholdConsumption consumption)
        {
            var min = TimeSeries.GetMinimumDate();
            TimeSeries.AddPointsFrom(consumption.DataPoints.Where(x => x.Time >= min), x => x.Time, x => x.Consumption, (val, pt) => pt.ActualConsumptionKwh = val);
            TimeSeriesUpdated?.Invoke(this, EventArgs.Empty);
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
