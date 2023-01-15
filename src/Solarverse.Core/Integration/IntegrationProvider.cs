namespace Solarverse.Core.Integration
{
    public class IntegrationProvider : IIntegrationProvider
    {
        public IntegrationProvider(IInverterClient inverterClient, ISolarForecastClient solarForecastClient, IEnergySupplierClient energySupplierClient)
        {
            InverterClient = inverterClient;
            SolarForecastClient = solarForecastClient;
            EnergySupplierClient = energySupplierClient;
        }

        public IInverterClient InverterClient { get; }

        public ISolarForecastClient SolarForecastClient { get; }

        public IEnergySupplierClient EnergySupplierClient { get; }
    }
}
