namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class AllSettingsTests
    {
        private AllSettings _testClass;

        public AllSettingsTests()
        {
            _testClass = new AllSettings();
        }

        [Fact]
        public void CanSetAndGetSettings()
        {
            // Arrange
            var testValue = new List<Setting>();

            // Act
            _testClass.Settings = testValue;

            // Assert
            _testClass.Settings.Should().BeSameAs(testValue);
        }
    }
}