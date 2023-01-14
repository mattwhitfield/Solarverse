using Solarverse.Core.Integration.GivEnergy.Models;

namespace Solarverse.Core.Integration.GivEnergy
{
    public interface IGivEnergyClient
    {
        Task<string> FindInverterSerial();
        Task<IEnumerable<Setting>> GetAllSettings();
        Task<CurrentState> GetCurrentState();
        Task<int> GetSettingId(string settingName);
        Task<object?> ReadSetting(int id);
        Task<SettingMutationValues?> SetSetting(int id, object value);
    }
}