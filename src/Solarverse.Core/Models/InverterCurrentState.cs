using System.Diagnostics;

namespace Solarverse.Core.Models
{
    [DebuggerDisplay("UpdateTime = {UpdateTime}, CurrentSolarPower = {CurrentSolarPower}, BatteryPercent = {BatteryPercent}, MaxDischargeRateKw = {MaxDischargeRateKw}, MaxChargeRateKw = {MaxChargeRateKw}, BatteryReserve = {BatteryReserve}")]
    public class InverterCurrentState
    {
        public static InverterCurrentState Default =>
            new InverterCurrentState(DateTime.MinValue, 0, 0, 2.6, 2.6, 4);

        public InverterCurrentState(DateTime updateTime, int currentSolarPower, int batteryPercent, double maxDischargeRateKw, double maxChargeRateKw, int batteryReserve)
        {
            UpdateTime = updateTime;
            CurrentSolarPower = currentSolarPower;
            BatteryPercent = batteryPercent;
            MaxDischargeRateKw = maxDischargeRateKw;
            MaxChargeRateKw = maxChargeRateKw;
            BatteryReserve = batteryReserve;
        }

        public DateTime UpdateTime { get; }

        public int CurrentSolarPower { get; }

        public int BatteryPercent { get; }

        public double MaxDischargeRateKw { get; }

        public double MaxChargeRateKw { get; }

        public int BatteryReserve { get; }
    }
}
