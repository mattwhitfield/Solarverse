using System.Diagnostics;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class NormalizedConsumptionDataPoint
    {
        public NormalizedConsumptionDataPoint(DateTime time, double consumption, double solar, double import, double export, double charge, double discharge)
        {
            Time = time;
            Consumption = consumption;
            Solar = solar;
            Import = import;
            Export = export;
            Charge = charge;
            Discharge = discharge;
        }

        public DateTime Time { get; }

        public double Consumption { get; }

        public double Solar { get; }

        public double Import { get; }

        public double Export { get; }

        public double Charge { get; }

        public double Discharge { get; }
    }
}
