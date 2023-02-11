using Solarverse.Core.Models;

namespace Solarverse.Core.Data
{
    public interface ICurrentDataService
    {
        event EventHandler<EventArgs> TimeSeriesUpdated;

        event EventHandler<EventArgs> CurrentStateUpdated;

        InverterCurrentState CurrentState { get; }
        TimeSeries TimeSeries { get; }
        void Cull(TimeSpan deleteOlderThan);
        IDisposable LockForUpdate();
        void Update(HouseholdConsumption consumption);
        void Update(PredictedConsumption consumption);
        void Update(InverterCurrentState currentState);
        void Update(SolarForecast forecast);
        void UpdateIncomingRates(IList<TariffRate> incomingRates);
        void UpdateOutgoingRates(IList<TariffRate> outgoingRates);
        void UpdateCurrentState(Action<InverterCurrentState> updateAction);
        void RecalculateForecast();

    }
}
