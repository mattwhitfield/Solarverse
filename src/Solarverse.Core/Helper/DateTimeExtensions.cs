namespace Solarverse.Core.Helper
{
    public static class DateTimeExtensions
    {
        public static DateTime ToHalfHourPeriod(this DateTime d) 
        {
            var secondsIntoDay = (d - d.Date).TotalSeconds;
            var halfHourPeriodsIntoDay = Math.Floor(secondsIntoDay / 1800);
            return d.Date.AddHours(halfHourPeriodsIntoDay * 0.5);
        }
    }
}
