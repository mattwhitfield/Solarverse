namespace Solarverse.Core.Tests.Helper
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Helper;
    using Solarverse.Core.Models.Settings;
    using Xunit;

    public class DefaultConfigurationProviderTests
    {
        private DefaultConfigurationProvider _testClass;

        public DefaultConfigurationProviderTests()
        {
            _testClass = new DefaultConfigurationProvider();
        }

        [Fact]
        public void CanGetConfiguration()
        {
            // Assert
            _testClass.Configuration.Should().BeAssignableTo<Configuration>();
        }
    }
}