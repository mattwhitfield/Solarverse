namespace Solarverse.Core.Integration
{
    public interface IIntegrationProvider
    {
        IInverterClient InverterClient { get; }

        ISolarForecastClient SolarForecastClient { get; }

        IEnergySupplierClient EnergySupplierClient { get; }
    }
}
