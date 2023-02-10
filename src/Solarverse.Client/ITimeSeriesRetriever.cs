using System.Threading.Tasks;

namespace Solarverse.Client
{
    public interface ITimeSeriesRetriever
    {
        Task UpdateTimeSeries();
    }
}
