using Microsoft.Extensions.Options;
using Solarverse.Core.Models.Settings;
using IConfigurationProvider = Solarverse.Core.Helper.IConfigurationProvider;

namespace Solarverse.Server
{
    public class OptionsConfigurationProvider : IConfigurationProvider
    {
        private readonly IOptions<Configuration> _configuration;

        public OptionsConfigurationProvider(IOptions<Configuration> configuration)
        {
            _configuration = configuration;
        }

        public Configuration Configuration => _configuration.Value;
    }
}
