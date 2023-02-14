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
        public void CannotConstructWithNullLogger()
        {
            FluentActions.Invoking(() => new TimedAction(default(ILogger), new Period(TimeSpan.FromSeconds(66)), () => Task.FromResult(true), "TestValue1324448480")).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CannotConstructWithNullPeriod()
        {
            FluentActions.Invoking(() => new TimedAction(Substitute.For<ILogger>(), default(Period), () => Task.FromResult(true), "TestValue468323584")).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CannotConstructWithNullExecute()
        {
            FluentActions.Invoking(() => new TimedAction(Substitute.For<ILogger>(), new Period(TimeSpan.FromSeconds(294)), default(Func<Task<bool>>), "TestValue1878295085")).Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CannotConstructWithInvalidActionName(string value)
        {
            FluentActions.Invoking(() => new TimedAction(Substitute.For<ILogger>(), new Period(TimeSpan.FromSeconds(416)), () => Task.FromResult(false), value)).Should().Throw<ArgumentNullException>();
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