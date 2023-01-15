namespace Solarverse.Core.Helper
{
    public class Period
    {
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
            return GetNext(current).Subtract(_period);
        }

        public DateTime GetNext(DateTime last)
        {
            var next = last.Date + _offset;
            while (next <= last)
            {
                next += _period;
            }
            return next;
        }
    }
}
