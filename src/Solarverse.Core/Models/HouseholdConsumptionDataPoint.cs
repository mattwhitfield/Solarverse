using System.Diagnostics;

namespace Solarverse.Core.Models
{
    [DebuggerDisplay("Time = {Time}, Consumption = {Consumption}, Solar = {Solar}, Import = {Import}, Export = {Export}, Charge = {Charge}, Discharge = {Discharge}, BatteryPercentage = {BatteryPercentage}")]
    public class HouseholdConsumptionDataPoint
    {
        public HouseholdConsumptionDataPoint(DateTime time, double consumption, double solar, double import, double export, double cahrge, double discharge, double batteryPercentage)
        {
            Time = time;
            Consumption = consumption;
            Solar = solar;
            Import = import;
            Export = export;
            Charge = cahrge;
            Discharge = discharge;
            BatteryPercentage = batteryPercentage;
        }

        public DateTime Time { get; }

        public double Consumption { get; set; }

        public double Solar { get; set; }

        public double Import { get; set; }

        public double Export { get; set; }

        public double Charge { get; set; }

        public double Discharge { get; set; }

        public double BatteryPercentage { get; set; }
    }
}
