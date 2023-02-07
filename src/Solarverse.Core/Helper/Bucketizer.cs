namespace Solarverse.Core.Helper
{
    using Solarverse.Core.Data;

    public static class Bucketizer
    {
        public static BucketedList Bucketize(IEnumerable<TimeSeriesPoint> points, double kwhPerPeriod)
        {
            var dischargePoints = points
                .OrderByDescending(x => x.IncomingRate)
                .Select(x => (kwh: x.RequiredPowerKwh ?? 0, cost: x.IncomingRate ?? 0))
                .ToList();

            var inputPeriods = dischargePoints.ToList();
            var bucketSize = kwhPerPeriod;

            var bucketCosts = new BucketedList();
            List<(double kwh, double cost)> currentBucketPeriods = new List<(double kwh, double cost)>();
            double currentBucketTotalKwh = 0;
            while (inputPeriods.Count > 0)
            {
                var current = inputPeriods[0];
                var currentPowerKwh = current.kwh;
                var currentCost = current.cost;
                if (currentBucketTotalKwh + currentPowerKwh >= bucketSize)
                {
                    var neededFromThisBucket = bucketSize - currentBucketTotalKwh;
                    var leftFromThisBucket = currentPowerKwh - neededFromThisBucket;
                    if (leftFromThisBucket > 0)
                    {
                        inputPeriods[0] = (leftFromThisBucket, currentCost);
                    }
                    else
                    {
                        inputPeriods.RemoveAt(0);
                    }

                    currentBucketPeriods.Add((neededFromThisBucket, currentCost));
                    bucketCosts.Add(CalculateCost(currentBucketPeriods));
                    currentBucketPeriods.Clear();

                    currentBucketTotalKwh = 0;
                }
                else
                {
                    currentBucketPeriods.Add(current);
                    currentBucketTotalKwh += current.kwh;
                    inputPeriods.RemoveAt(0);
                }    
            }

            if (currentBucketPeriods.Count > 0)
            {
                bucketCosts.Add(CalculateCost(currentBucketPeriods));
            }

            return bucketCosts;
        }

        private static double CalculateCost(IList<(double kwh, double cost)> currentBucketPeriods)
        {
            var totalKwh = currentBucketPeriods.Sum(x => x.kwh);
            return currentBucketPeriods.Select(x => (x.kwh / totalKwh) * x.cost).Sum();
        }
    }
}
