namespace Solarverse.Core.Tests.Integration.Octopus.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.Octopus.Models;
    using Xunit;

    public class TariffTests
    {
        private Tariff _testClass;

        public TariffTests()
        {
            _testClass = new Tariff();
        }

        [Fact]
        public void CanSetAndGetCode()
        {
            // Arrange
            var testValue = "TestValue1620713074";

            // Act
            _testClass.Code = testValue;

            // Assert
            _testClass.Code.Should().Be(testValue);
        }
    }
}