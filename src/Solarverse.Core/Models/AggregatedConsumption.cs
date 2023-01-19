namespace Solarverse.Core.Models
{
    public class PredictedConsumption
    {
        private List<HouseholdConsumptionDataPoint> _dataPoints = new List<HouseholdConsumptionDataPoint>();

        public PredictedConsumption(IEnumerable<PredictedConsumption> sources)
        {
            foreach (var source in sources.Where(x => x.IsValid).OrderBy(x => x.DataPoints.Select(x => x.Time).Min()))
            {
                _dataPoints.AddRange(source.DataPoints);
            }

            IsValid = DataPoints.Any();
        }

        public PredictedConsumption(IEnumerable<HouseholdConsumption> sources, DateTime date)
        {
            var validSources = sources.Where(x => x.IsValid).ToList();

            if (validSources.Count == 0)
            {
                IsValid = false;
                return;
            }

            for (int i = 0; i < 48; i++)
            {
                var projectionDate = date.Add(validSources[0].DataPoints[i].Time.TimeOfDay);
                var pointValue = validSources.Sum(x => x.DataPoints[i].Consumption) / validSources.Count;
                DataPoints.Add(new HouseholdConsumptionDataPoint(projectionDate, pointValue));
            }
        }

        public PredictedConsumption LimitTo(DateTime from, DateTime to)
        {
            _dataPoints = _dataPoints.Where(x => x.Time >= from && x.Time <= to).ToList();
            return this;
        }

        public bool IsValid { get; } = true;

        public IList<HouseholdConsumptionDataPoint> DataPoints => _dataPoints;
    }
}
