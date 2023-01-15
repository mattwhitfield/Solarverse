namespace Solarverse.Core.Control
{
    public interface IControlLoop
    {
        Task Run(CancellationToken cancellation);
    }
}