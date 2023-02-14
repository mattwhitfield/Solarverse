namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class SettingMutationTests
    {
        private SettingMutation _testClass;

        public SettingMutationTests()
        {
            _testClass = new SettingMutation();
        }

        [Fact]
        public void CanSetAndGetData()
        {
            // Arrange
            var testValue = new SettingMutationValues { Value = new object(), Success = true, Message = "TestValue1110341386" };

            // Act
            _testClass.Data = testValue;

            // Assert
            _testClass.Data.Should().BeSameAs(testValue);
        }
    }
}