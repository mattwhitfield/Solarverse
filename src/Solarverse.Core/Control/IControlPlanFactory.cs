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
            // TODO
        }

        public void CreatePlan()
        {
            // TODO
        }
    }
}
