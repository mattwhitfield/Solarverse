using Solarverse.Core.Models.Settings;

namespace Solarverse.Core.Helper
{
    public interface IConfigurationProvider
    {
        Configuration Configuration { get; }
    }
}