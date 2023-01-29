using Solarverse.Core.Models;

namespace Solarverse.Core.Control
{
    public interface IControlPlanFactory
    {
        void CreatePlan();

        void CheckForAdaptations(InverterCurrentState currentState);
    }
}
