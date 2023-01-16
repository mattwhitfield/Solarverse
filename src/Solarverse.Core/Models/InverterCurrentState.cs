using Solarverse.Core.Data;

namespace Solarverse.Core.Models
{
    public class InverterCurrentState
    {
        public static InverterCurrentState Default =>
            new InverterCurrentState(DateTime.MinValue, 0, 0, 0, 0);

        public InverterCurrentState(DateTime updateTime, int currentSolarPower, int batteryPercent, double maxDischargeRateKw, double maxChargeRateKw)
        {
            UpdateTime = updateTime;
            CurrentSolarPower = currentSolarPower;
            BatteryPercent = batteryPercent;
            MaxDischargeRateKw = maxDischargeRateKw;
            MaxChargeRateKw = maxChargeRateKw;
        }

        public DateTime UpdateTime { get; }

        public int CurrentSolarPower { get; }

        public int BatteryPercent { get; }

        public double MaxDischargeRateKw { get; }

        public double MaxChargeRateKw { get; }
    }
}
