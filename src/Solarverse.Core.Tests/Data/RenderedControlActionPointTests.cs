namespace Solarverse.Core.Tests.Data
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Data;
    using Xunit;

    public class RenderedControlActionPointTests
    {
        private RenderedControlActionPoint _testClass;
        private DateTime _time;
        private ControlAction _value;

        public RenderedControlActionPointTests()
        {
            _time = DateTime.UtcNow;
            _value = ControlAction.Hold;
            _testClass = new RenderedControlActionPoint(_time, _value);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new RenderedControlActionPoint(_time, _value);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void TimeIsInitializedCorrectly()
        {
            _testClass.Time.Should().Be(_time);
        }

        [Fact]
        public void ValueIsInitializedCorrectly()
        {
            _testClass.Value.Should().Be(_value);
        }
    }
}