namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class SettingValueTests
    {
        private SettingValue _testClass;

        public SettingValueTests()
        {
            _testClass = new SettingValue();
        }

        [Fact]
        public void CanSetAndGetValue()
        {
            // Arrange
            var testValue = new object();

            // Act
            _testClass.Value = testValue;

            // Assert
            _testClass.Value.Should().BeSameAs(testValue);
        }
    }
}