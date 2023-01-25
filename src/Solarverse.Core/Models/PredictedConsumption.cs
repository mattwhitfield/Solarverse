using System.Diagnostics;

namespace Solarverse.Core.Models
{
    [DebuggerDisplay("IsValid = {IsValid}, DataPoints: {DataPoints.Count}")]
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
                var pointConsumptionValue = validSources.Sum(x => x.DataPoints[i].Consumption) / validSources.Count;
                var pointSolarValue = validSources.Sum(x => x.DataPoints[i].Solar) / validSources.Count;
                var pointImportValue = validSources.Sum(x => x.DataPoints[i].Import) / validSources.Count;
                var pointExportValue = validSources.Sum(x => x.DataPoints[i].Export) / validSources.Count;
                var pointChargeValue = validSources.Sum(x => x.DataPoints[i].Charge) / validSources.Count;
                var pointDischargeValue = validSources.Sum(x => x.DataPoints[i].Discharge) / validSources.Count;

                DataPoints.Add(new HouseholdConsumptionDataPoint(
                    projectionDate,
                    pointConsumptionValue,
                    pointSolarValue,
                    pointImportValue,
                    pointExportValue,
                    pointChargeValue,
                    pointDischargeValue,
                    0)); // this is not where we predict battery percentages
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
