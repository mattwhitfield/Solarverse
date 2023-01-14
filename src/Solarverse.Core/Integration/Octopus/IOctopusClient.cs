using Solarverse.Core.Integration.Octopus.Models;

namespace Solarverse.Core.Integration.Octopus
{
    public interface IOctopusClient
    {
        Task<AgileRates> GetAgileRates(string productCode, string mpan);
    }
}