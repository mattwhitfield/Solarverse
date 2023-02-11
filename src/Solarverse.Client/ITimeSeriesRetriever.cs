using System.Threading.Tasks;

namespace Solarverse.Client
{
    public interface ISolarverseApiClient
    {
        Task UpdateTimeSeries();
        Task UpdateCurrentState();
        Task UpdateMemoryLog();
    }
}
