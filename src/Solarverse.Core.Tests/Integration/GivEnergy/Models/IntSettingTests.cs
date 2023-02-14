namespace Solarverse.Core.Tests.Integration.GivEnergy.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Integration.GivEnergy.Models;
    using Xunit;

    public class IntSettingTests
    {
        private IntSetting _testClass;
        private int _id;
        private int _value;

        public IntSettingTests()
        {
            _id = 1577514368;
            _value = 2132150551;
            _testClass = new IntSetting(_id, _value);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new IntSetting(_id, _value);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void IdIsInitializedCorrectly()
        {
            _testClass.Id.Should().Be(_id);
        }

        [Fact]
        public void ValueIsInitializedCorrectly()
        {
            _testClass.Value.Should().Be(_value);
        }
    }
}