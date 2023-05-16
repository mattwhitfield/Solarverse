using Solarverse.Core.Models;

namespace Solarverse.Core.Integration
{
    public interface IInverterClient
    {
        Task<HouseholdConsumption> GetHouseholdConsumptionFor(DateTime date);
        Task<InverterCurrentState?> GetCurrentState();
        Task Charge(DateTime until);
        Task Hold(DateTime until);
        Task Discharge(DateTime until);
        Task Export(DateTime until);
    }
}