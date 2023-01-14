namespace Solarverse.Core.Tests.Integration.Solcast
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Solarverse.Core.Helper;
    using Solarverse.Core.Integration.Solcast;
    using Xunit;

    public class SolcastClientTests
    {
        private SolcastClient _testClass;
        private string _key;

        public SolcastClientTests()
        {
            _key = ConfigurationProvider.Configuration.ApiKeys.Solcast;
            _testClass = new SolcastClient(_key);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new SolcastClient(_key);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public async Task CanCallGetForecastSet()
        {
            // Arrange
            var siteId = "835c-aa5e-6257-df6d";

            // Act
            var result = await _testClass.GetForecastSet(siteId);

        }

    }
}