namespace Solarverse.Core.Integration.GivEnergy
{
    public static class SettingIds
    {
        public const int EcoMode = 24; // true / false

        public static class Charge
        {
            public const int StartTime = 64; // HH:mm
            public const int EndTime = 65; // HH:mm
            public const int Enabled = 66; // true / false
            public const int PowerLimit = 72; // int
            public const int TargetPercent = 77; // int
            public const int TargetEnabled = 17; // true / false
        }

        public static class Discharge
        {
            public const int StartTime = 53; // HH:mm
            public const int EndTime = 54; // HH:mm
            public const int Enabled = 56; // true / false
            public const int PowerLimit = 73; // int
            public const int Reserve = 71;
        }

        public static string GetName(int settingId)
        {
            return settingId switch
            {
                24 => "EcoMode",
                64 => "Charge.StartTime",
                65 => "Charge.EndTime",
                66 => "Charge.Enabled",
                72 => "Charge.PowerLimit",
                77 => "Charge.TargetPercent",
                17 => "Charge.TargetEnabled",
                53 => "Discharge.StartTime",
                54 => "Discharge.EndTime",
                56 => "Discharge.Enabled",
                73 => "Discharge.PowerLimit",
                71 => "Discharge.Reserve",
                _ => "Unknown",
            };
        }
    }
}
