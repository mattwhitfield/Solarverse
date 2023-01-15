﻿namespace Solarverse.Core.Models
{
    public class HouseholdConsumption
    {
        public HouseholdConsumption(bool isValid, bool containsInterpolatedPoints, IEnumerable<HouseholdConsumptionDataPoint> dataPoints)
        {
            IsValid = isValid;
            ContainsInterpolatedPoints = containsInterpolatedPoints;
            DataPoints = dataPoints.ToList();
        }

        public bool ContainsInterpolatedPoints { get; }

        public bool IsValid { get; }

        public IList<HouseholdConsumptionDataPoint> DataPoints { get; }
    }
}
