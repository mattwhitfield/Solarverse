namespace Solarverse.Core.Tests.Helper
{
    using System;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using Solarverse.Core.Helper;
    using Xunit;
    using TState = System.String;

    public class MemoryLoggerTests
    {
        private MemoryLogger _testClass;
        private string _name;
        private IMemoryLog _memoryLog;

        public MemoryLoggerTests()
        {
            _name = "TestValue564994430";
            _memoryLog = Substitute.For<IMemoryLog>();
            _testClass = new MemoryLogger(_name, _memoryLog);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new MemoryLogger(_name, _memoryLog);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void CannotConstructWithNullMemoryLog()
        {
            FluentActions.Invoking(() => new MemoryLogger("TestValue1962455977", default(IMemoryLog))).Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CannotConstructWithInvalidName(string value)
        {
            FluentActions.Invoking(() => new MemoryLogger(value, Substitute.For<IMemoryLog>())).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallBeginScope()
        {
            // Arrange
            var state = "TestValue1140174503";

            // Act
            var result = _testClass.BeginScope<TState>(state);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallIsEnabled()
        {
            // Arrange
            var logLevel = LogLevel.Warning;

            // Act
            var result = _testClass.IsEnabled(logLevel);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CanCallLog()
        {
            // Arrange
            var logLevel = LogLevel.None;
            var eventId = new EventId();
            var state = "TestValue804120236";
            var exception = new Exception();
            Func<TState, Exception, string> formatter = (x, y) => "TestValue1159690945";

            // Act
            _testClass.Log<TState>(logLevel, eventId, state, exception, formatter);

            // Assert
            _memoryLog.Received().Add(Arg.Any<string>());

            throw new NotImplementedException("Create or modify test");
        }

        [Fact]
        public void CannotCallLogWithNullFormatter()
        {
            FluentActions.Invoking(() => _testClass.Log<TState>(LogLevel.None, new EventId(), "TestValue1242567081", new Exception(), default(Func<TState, Exception, string>))).Should().Throw<ArgumentNullException>();
        }
    }
}