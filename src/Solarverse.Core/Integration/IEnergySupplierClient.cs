using Solarverse.Core.Models;

namespace Solarverse.Core.Integration
{
    public interface IEnergySupplierClient
    {
        Task<IList<TariffRate>> GetTariffRates(string productCode, string mpan);
    }
}