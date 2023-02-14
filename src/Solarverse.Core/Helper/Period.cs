namespace Solarverse.Core.Helper
{
    public class Period
    {
        public static Period HalfHourly = new Period(TimeSpan.FromMinutes(30));

        private readonly TimeSpan _period;
        private readonly TimeSpan _offset;

        public Period(TimeSpan period)
            : this(period, TimeSpan.Zero)
        { }

        public Period(TimeSpan period, TimeSpan offset)
        {
            _period = period;
            _offset = offset;
        }

        public DateTime GetLast(DateTime current)
        {
            return GetNext(current) - _period;
        }

        public DateTime GetNext(DateTime last)
        {
            var secondsIntoDay = last.TimeOfDay.TotalSeconds - _offset.TotalSeconds;
            var periodsIntoDay = secondsIntoDay / _period.TotalSeconds;
            return last.Date + ((Math.Floor(periodsIntoDay) + 1) * _period) + _offset;
        }
    }
}
