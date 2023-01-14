namespace Solarverse.Core.Helper
{
    public class Period
    {
        private readonly TimeSpan _period;
        private readonly TimeSpan _offset;

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
            var current = last.Date;
            while (current + _offset <= last)
            {
                current += _period;
            }
            return current + _offset;
        }
    }
}
