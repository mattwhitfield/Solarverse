using System.Diagnostics;

namespace Solarverse.Core.Integration.GivEnergy.Models
{
    [DebuggerDisplay("IsValid = {IsValid}, DataPoints: {DataPoints.Count}")]
    public class NormalizedConsumption
    {
        public NormalizedConsumption(ConsumptionHistory history)
        {
            if (history.DataPoints == null || !history.DataPoints.Any())
            {
                IsValid = false;
                return;
            }

            var orderedPoints = history.DataPoints.OrderBy(x => x.Time).ToList();
            int i = 0;
            while (i < orderedPoints.Count - 1)
            {
                var current = orderedPoints[i].Today;
                var next = orderedPoints[i + 1].Today;
                if (current != null && next != null && current.Consumption > next.Consumption)
                {
                    orderedPoints.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            var datapoints = orderedPoints;
            var cumulativePoints = GetCumulativePoints(datapoints);

            double lastConsumption = 0d,
                   lastSolar = 0d,
                   lastImport = 0d,
                   lastExport = 0d,
                   lastCharge = 0d,
                   lastDischarge = 0d;
            foreach (var point in cumulativePoints) 
            {
                if (point == null)
                {
                    IsValid = false;
                    return;
                }

                DataPoints.Add(new NormalizedConsumptionDataPoint(
                    point.Time,
                    point.Consumption - lastConsumption,
                    point.Solar - lastSolar,
                    point.Import - lastImport,
                    point.Export - lastExport,
                    point.Charge - lastCharge,
                    point.Discharge - lastDischarge,
                    point.BatteryPercentage));
                lastConsumption = point.Consumption;
                lastSolar = point.Solar;
                lastImport = point.Import;
                lastExport = point.Export;
                lastCharge = point.Charge;
                lastDischarge = point.Discharge;
            }
        }

        private static NormalizedConsumptionDataPoint?[] GetCumulativePoints(List<ConsumptionDataPoint> datapoints)
        {
            var cumulativePoints = new NormalizedConsumptionDataPoint?[48];
            var date = datapoints[0].Time.Date;

            foreach (var dataPoint in datapoints.GroupBy(x => (int)((x.Time - date).TotalMinutes / 30)))
            {
                cumulativePoints[dataPoint.Key] = new NormalizedConsumptionDataPoint(
                    date.AddMinutes(dataPoint.Key * 30),
                    dataPoint.Max(x => x.Today?.Consumption ?? 0),
                    dataPoint.Max(x => x.Today?.Solar ?? 0),
                    dataPoint.Max(x => x.Today?.Grid?.Import ?? 0),
                    dataPoint.Max(x => x.Today?.Grid?.Export ?? 0),
                    dataPoint.Max(x => x.Today?.Battery?.Charge ?? 0),
                    dataPoint.Max(x => x.Today?.Battery?.Discharge ?? 0),
                    dataPoint.Where(x => x.Power?.Battery?.Percent > 0).OrderByDescending(x => x.Time).Select(x => x.Power?.Battery?.Percent ?? 0).FirstOrDefault());
            }

            for (int i = 0; i < 48; i++)
            {
                if (cumulativePoints[i] == null)
                {
                    var prev = FindPrev(i, cumulativePoints);
                    var next = FindNext(i, cumulativePoints);

                    if (prev == null || next == null)
                    {
                        continue;
                    }

                    var targetMinutes = i * 30.0;
                    var prevMinutes = (prev.Time - date).TotalMinutes;
                    var nextMinutes = (next.Time - date).TotalMinutes;

                    // inverse interpolation to find where we should be
                    var factor = (targetMinutes - prevMinutes) / (nextMinutes - prevMinutes);

                    double Interpolate(Func<NormalizedConsumptionDataPoint, double> selector)
                    {
                        return (1 - factor) * selector(prev) + factor * selector(next);
                    }

                    cumulativePoints[i] = new NormalizedConsumptionDataPoint(
                        date.AddMinutes(i * 30), 
                        Interpolate(x => x.Consumption),
                        Interpolate(x => x.Solar),
                        Interpolate(x => x.Import),
                        Interpolate(x => x.Export),
                        Interpolate(x => x.Charge),
                        Interpolate(x => x.Discharge),
                        Interpolate(x => x.BatteryPercentage));
                }
            }

            return cumulativePoints;
        }

        private static NormalizedConsumptionDataPoint? FindPrev(int currentIndex, NormalizedConsumptionDataPoint?[] source)
        {
            while (currentIndex >= 0)
            {
                if (source[currentIndex] != null)
                {
                    return source[currentIndex];
                }
                currentIndex--;
            }

            return null;
        }

        private static NormalizedConsumptionDataPoint? FindNext(int currentIndex, NormalizedConsumptionDataPoint?[] source)
        {
            while (currentIndex < 48)
            {
                if (source[currentIndex] != null)
                {
                    return source[currentIndex];
                }
                currentIndex++;
            }

            return null;
        }

        public bool ContainsInterpolatedPoints { get; }

        public bool IsValid { get; } = true;

        public List<NormalizedConsumptionDataPoint> DataPoints { get; } = new List<NormalizedConsumptionDataPoint>();
    }
}
