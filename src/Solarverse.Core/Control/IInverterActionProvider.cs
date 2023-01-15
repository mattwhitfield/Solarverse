namespace Solarverse.Core.Control
{
    public interface IInverterActionProvider
    {
        Task ChargeUntil(DateTime endTime);

        Task Discharge();

        Task ExportUntil(DateTime endTime);

        Task Hold();
    }
}