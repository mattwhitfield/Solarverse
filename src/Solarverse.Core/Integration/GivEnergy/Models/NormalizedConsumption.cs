﻿namespace Solarverse.Core.Integration.GivEnergy.Models
{
    public class NormalizedConsumption
    {
        public NormalizedConsumption(ConsumptionHistory history)
        {
            if (history.DataPoints == null || !history.DataPoints.Any())
            {
                IsValid = false;
                return;
            }

            var datapoints = history.DataPoints;
            var cumulativePoints = GetCumulativePoints(datapoints);

            var last = 0d;
            foreach (var point in cumulativePoints) 
            {
                if (point == null)
                {
                    IsValid = false;
                    return;
                }

                DataPoints.Add(new NormalizedConsumptionDataPoint(point.Time, point.Consumption - last));
                last = point.Consumption;
            }
        }

        private NormalizedConsumptionDataPoint?[] GetCumulativePoints(List<ConsumptionDataPoint> datapoints)
        {
            var cumulativePoints = new NormalizedConsumptionDataPoint?[48];
            var date = datapoints[0].Time.Date;

            foreach (var dataPoint in datapoints.GroupBy(x => (int)((x.Time - date).TotalMinutes / 30)))
            {
                cumulativePoints[dataPoint.Key] = new NormalizedConsumptionDataPoint(date.AddMinutes(dataPoint.Key * 30), dataPoint.Max(x => x.Today?.Consumption ?? double.MaxValue));
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

                    var interpolated = (1 - factor) * prev.Consumption + factor * next.Consumption;

                    cumulativePoints[i] = new NormalizedConsumptionDataPoint(date.AddMinutes(i * 30), interpolated);
                }
            }

            return cumulativePoints;
        }

        private NormalizedConsumptionDataPoint? FindPrev(int currentIndex, NormalizedConsumptionDataPoint?[] source)
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

        private NormalizedConsumptionDataPoint? FindNext(int currentIndex, NormalizedConsumptionDataPoint?[] source)
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
