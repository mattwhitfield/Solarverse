namespace Solarverse.Core.Control
{
    public interface IControlPlanExecutor
    {
        Task<bool> ExecutePlan();
    }
}
