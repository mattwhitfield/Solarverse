namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class BoolSetting
    {
        public BoolSetting(int id, bool value)
        {
            Id = id;
            Value = value;
        }

        public int Id { get; }

        public bool Value { get; }
    }
}
