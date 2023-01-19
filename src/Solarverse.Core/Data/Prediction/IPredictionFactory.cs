using Solarverse.Core.Models;

namespace Solarverse.Core.Data.Prediction
{
    public interface IPredictionFactory
    {
        Task<PredictedConsumption> CreatePredictionFrom(DateTime from, DateTime to);
    }
}