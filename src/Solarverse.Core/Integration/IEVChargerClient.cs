namespace Solarverse.Core.Integration
{
    public interface IEVChargerClient
    {
        Task SetChargingEnabled(bool enabled);
    }
}