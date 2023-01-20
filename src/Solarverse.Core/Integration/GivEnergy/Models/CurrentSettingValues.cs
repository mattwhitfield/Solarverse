namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class CurrentSettingValues
    {
        public CurrentSettingValues(BoolSetting ecoModeEnabled, BatteryModeSettingValues chargeSettings, BatteryModeSettingValues dischargeSettings)
        {
            EcoModeEnabled = ecoModeEnabled;
            ChargeSettings = chargeSettings;
            DischargeSettings = dischargeSettings;
        }

        public BoolSetting EcoModeEnabled { get; }

        public BatteryModeSettingValues ChargeSettings { get; }

        public BatteryModeSettingValues DischargeSettings { get; }
    }
}
