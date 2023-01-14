using Solarverse.Core.Integration.Solcast.Models;

namespace Solarverse.Core.Integration.Solcast
{
    public interface ISolcastClient
    {
        Task<ForecastSet> GetForecastSet(string siteId);
    }
}