using Solarverse.Core.Models;

namespace Solarverse.Core.Control
{
    public interface IControlPlanFactory
    {
        void CreatePlan();

        void CheckForAdaptations(InverterCurrentState currentState);
    }

    public class ControlPlanFactory : IControlPlanFactory
    {
        public void CheckForAdaptations(InverterCurrentState currentState)
        {
            // happens after every current state update - here we check if the battery charge is on track and if we need another 1/2 hour charge period
            // TODO
        }

        public void CreatePlan()
        {
            // happns after tariffs become available, so we look at solar forecast, predicted consumption and tariff rates
            // TODO
        }
    }
}
