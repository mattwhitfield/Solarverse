namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class IntSetting
    {
        public IntSetting(int id, int value)
        {
            Id = id;
            Value = value;
        }

        public int Id { get; }

        public int Value { get; }
    }
}
