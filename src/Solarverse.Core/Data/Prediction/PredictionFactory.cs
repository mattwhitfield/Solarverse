﻿using Solarverse.Core.Helper;
using Solarverse.Core.Models;

namespace Solarverse.Core.Data.Prediction
{
    public class PredictionFactory : IPredictionFactory
    {
        private readonly IDataStore _dataStore;

        public PredictionFactory(IDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public async Task<PredictedConsumption> CreatePredictionFrom(DateTime from, DateTime to)
        {
            Func<DateTime, IEnumerable<DateTime>> dateSelector;

            var predictionSettings = ConfigurationProvider.Configuration.Prediction;
            switch (predictionSettings.MethodName ?? string.Empty)
            {
                default:
                case "LastNSameDayOfWeek":
                    dateSelector = date => LastNSameDayOfWeek(date, predictionSettings.NumberOfDays);
                    break;

                case "LastNDays":
                    dateSelector = date => LastNDays(date, predictionSettings.NumberOfDays);
                    break;
            }

            List<PredictedConsumption> aggregateList = new List<PredictedConsumption>();
            foreach (var date in GetDates(from, to))
            {
                List<HouseholdConsumption> consumptionList = new List<HouseholdConsumption>();
                foreach (var sourceDate in dateSelector(date))
                {
                    consumptionList.Add(await _dataStore.GetHouseholdConsumptionFor(sourceDate));
                }
                aggregateList.Add(new PredictedConsumption(consumptionList, date));
            }

            return new PredictedConsumption(aggregateList).LimitTo(from, to);
        }

        private static IEnumerable<DateTime> GetDates(DateTime from, DateTime to)
        {
            var current = from.Date;
            var last = to.Date;
            while (current <= last)
            {
                yield return current;
                current = current.AddDays(1);
            }
        }

        private static IEnumerable<DateTime> LastNSameDayOfWeek(DateTime date, int numberOfDays)
        {
            var i = 0;
            while (i < numberOfDays)
            {
                date = date.AddDays(-7);
                yield return date;
                i++;
            }
        }

        private static IEnumerable<DateTime> LastNDays(DateTime date, int numberOfDays)
        {
            var i = 0;
            while (i < numberOfDays)
            {
                date = date.AddDays(-7);
                yield return date;
                i++;
            }
        }
    }
}