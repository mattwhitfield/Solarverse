using Solarverse.Core.Data;

namespace Solarverse.Core.Models
{
    public class InverterCurrentState
    {
        public static InverterCurrentState Default =>
            new InverterCurrentState(DateTime.MinValue, 0, 0, ControlAction.Hold);

        public InverterCurrentState(DateTime updateTime, int currentSolarPower, int batteryPercent, ControlAction currentAction)
        {
            UpdateTime = updateTime;
            CurrentSolarPower = currentSolarPower;
            BatteryPercent = batteryPercent;
            CurrentAction = currentAction;
        }

        public DateTime UpdateTime { get; }

        public int CurrentSolarPower { get; }

        public int BatteryPercent { get; }

        public ControlAction CurrentAction { get; }
    }
}
