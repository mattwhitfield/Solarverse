namespace Solarverse.Core.Helper
{
    public class BucketedList : List<double>
    {
        public bool TakeSlot(double? incomingRate, double efficiency)
        {
            var eligible = this.Where(x => x * efficiency > (incomingRate ?? 0)).OrderByDescending(x => x).ToList();
            if (eligible.Any())
            {
                var rate = eligible.First();
                var index = IndexOf(rate);
                RemoveAt(index);
                return true;
            }

            return false;
        }
    }
}
