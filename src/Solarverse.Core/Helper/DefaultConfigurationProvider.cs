using Solarverse.Core.Models.Settings;

namespace Solarverse.Core.Helper
{
    public class DefaultConfigurationProvider : IConfigurationProvider
    {
        public Configuration Configuration => StaticConfigurationProvider.Configuration;
    }
}
