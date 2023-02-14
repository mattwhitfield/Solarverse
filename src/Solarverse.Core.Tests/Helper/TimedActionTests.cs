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

        public TimedActionTests()
        {
            _logger = Substitute.For<ILogger>();
            _period = new Period(TimeSpan.FromSeconds(79));
            _execute = () => Task.FromResult(true);
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
            var current = DateTime.UtcNow;

            // Act
            var result = await _testClass.Run(current);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }
    }
}