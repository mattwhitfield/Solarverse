namespace Solarverse.Core.Helper
{
    public class CurrentTimeProvider : ICurrentTimeProvider
    {
        private TimeZoneInfo _timeZone;

        public CurrentTimeProvider(IConfigurationProvider configurationProvider)
        {
            var timeZoneId = configurationProvider.Configuration.TimeZoneName;
            try
            {
                _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new InvalidOperationException("Could not find the time zone '" + timeZoneId + "'");
            }
        }

        public DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, _timeZone);

        public DateTime UtcNow => DateTime.UtcNow;

        public DateTime CurrentPeriodStartUtc => Period.HalfHourly.GetLast(DateTime.UtcNow);

        public TimeSpan Offset
        {
            get
            {
                var utcNow = UtcNow;
                var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _timeZone);
                return localNow - utcNow;
            }
        }

        public DateTime ToLocalTime(DateTime utcTime) => TimeZoneInfo.ConvertTimeFromUtc(utcTime, _timeZone);

        public TimeSpan ToLocalTime(TimeSpan utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.Date.Add(utcTime), _timeZone).TimeOfDay;
        }
    }
}
