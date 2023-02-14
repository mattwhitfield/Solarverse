namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;
    using Array = Core.Integration.GivEnergy.Models.Array;

    public class CurrentStateTests
    {
        private CurrentState _testClass;

        public CurrentStateTests()
        {
            _testClass = new CurrentState();
        }

        [Fact]
        public void CanSetAndGetData()
        {
            // Arrange
            var testValue = new CurrentStateData { Time = DateTime.UtcNow, Solar = new Solar { Power = 30722235, Arrays = new List<Array>() }, Battery = new Battery { Percent = 1418842837, Power = 979649970, Temperature = 1726129998.45 } };

            // Act
            _testClass.Data = testValue;

            // Assert
            _testClass.Data.Should().BeSameAs(testValue);
        }
    }
}