namespace Solarverse.Core.Tests.Helper
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using Solarverse.Core.Helper;
    using Xunit;

    public class TimedActionTests
    {
        private TimedAction _testClass;
        private ILogger _logger;
        private Period _period;
        private Func<Task<bool>> _execute;
        private string _actionName;
        private bool _executeCalled;
        private bool _executeResult = true;
        private DateTime _initialTime;

        public TimedActionTests()
        {
            _logger = Substitute.For<ILogger>();
            _period = new Period(TimeSpan.FromSeconds(60));
            _initialTime = DateTime.UtcNow;
            _execute = () =>
            {
                _executeCalled = true;
                return Task.FromResult(_executeResult);
            };
            _actionName = "TestValue716005873";
            _testClass = new TimedAction(_logger, _period, _execute, _actionName);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new TimedAction(_logger, _period, _execute, _actionName);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public async Task CanCallRun()
        {
            // Arrange
            var current = _period.GetLast(_initialTime);

            // Act
            var result = await _testClass.Run(current);

            // Assert
            _executeCalled.Should().BeTrue();
            result.Should().Be(TimedActionResult.Succeeded);

            // Arrange
            _executeCalled = false;

            // Act
            result = await _testClass.Run(current);

            // Assert
            _executeCalled.Should().BeFalse();
            result.Should().Be(TimedActionResult.NotDue);

            // Arrange
            current = current.AddMinutes(1);
            _executeResult = false;
            _executeCalled = false;

            // Act
            result = await _testClass.Run(current);

            // Assert
            _executeCalled.Should().BeTrue();
            result.Should().Be(TimedActionResult.Failed);

            // Arrange
            _executeCalled = false;

            // Act
            result = await _testClass.Run(current);

            // Assert
            _executeCalled.Should().BeFalse();
            result.Should().Be(TimedActionResult.NotDue);

            // Arrange
            current = current.AddSeconds(15);
            _executeResult = true;
            _executeCalled = false;

            // Act
            result = await _testClass.Run(current);

            // Assert
            _executeCalled.Should().BeTrue();
            result.Should().Be(TimedActionResult.Succeeded);
        }
    }
}