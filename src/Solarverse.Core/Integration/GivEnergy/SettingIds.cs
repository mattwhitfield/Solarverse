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
    }
}
