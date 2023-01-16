namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class TimeSetting
    {
        public TimeSetting(int id, TimeSpan? value)
        {
            Id = id;
            Value = value;
        }

        public int Id { get; }

        public TimeSpan? Value { get; }
    }
}
