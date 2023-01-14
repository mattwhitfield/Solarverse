namespace Solarverse.Core.Tests.Integration.Octopus
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Solarverse.Core.Helper;
    using Solarverse.Core.Integration.Octopus;
    using Solarverse.Core.Models;
    using Xunit;

    public class OctopusClientTests
    {
        private string _key;
        private OctopusClient _testClass;

        public OctopusClientTests()
        {
            _key = ConfigurationProvider.Configuration.ApiKeys.Octopus;
            _testClass = new OctopusClient(_key);
        }

        [Fact]
        public async Task CanCallGetIncomingAgileRates()
        {
            // Act
            var incoming = ConfigurationProvider.Configuration.IncomingMeter;
            var result = await _testClass.GetAgileRates(incoming.TariffName, incoming.MPAN);

            // Assert
        }

    }
}