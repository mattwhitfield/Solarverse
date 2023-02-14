namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class SettingValueDataTests
    {
        private SettingValueData _testClass;

        public SettingValueDataTests()
        {
            _testClass = new SettingValueData();
        }

        [Fact]
        public void CanSetAndGetData()
        {
            // Arrange
            var testValue = new SettingValue { Value = new object() };

            // Act
            _testClass.Data = testValue;

            // Assert
            _testClass.Data.Should().BeSameAs(testValue);
        }
    }
}