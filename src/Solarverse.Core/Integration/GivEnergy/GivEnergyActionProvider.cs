using Solarverse.Core.Control;

namespace Solarverse.Core.Integration.GivEnergy
{
    public class GivEnergyActionProvider : IInverterActionProvider
    {
        private readonly IInverterClient _givEnergyClient;

        public GivEnergyActionProvider(IInverterClient givEnergyClient)
        {
            _givEnergyClient = givEnergyClient;
        }

        public async Task ChargeUntil(DateTime endTime)
        {
            // TODO
        }

        public async Task Hold()
        {
            // TODO
        }

        public async Task Discharge()
        {
            // TODO
        }

        public async Task ExportUntil(DateTime endTime)
        {
            // TODO
        }
    }
}
