namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class BatteryModeSettingValues
    {
        public BatteryModeSettingValues(TimeSetting startTime, TimeSetting endTime, BoolSetting enabled, IntSetting powerLimit)
        {
            StartTime = startTime;
            EndTime = endTime;
            Enabled = enabled;
            PowerLimit = powerLimit;
        }

        public TimeSetting StartTime { get; }

        public TimeSetting EndTime { get; }

        public BoolSetting Enabled { get; }

        public IntSetting PowerLimit { get; }
    }
}
