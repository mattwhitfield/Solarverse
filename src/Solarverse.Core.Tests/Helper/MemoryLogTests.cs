namespace Solarverse.Core.Tests.Helper
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Helper;
    using Xunit;

    public class MemoryLogTests
    {
        private MemoryLog _testClass;

        public MemoryLogTests()
        {
            _testClass = new MemoryLog();
        }

        [Fact]
        public void CanCallAdd()
        {
            // Arrange
            var logLine = "TestValue1972192104";

            // Act
            _testClass.Add(logLine);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CannotCallAddWithInvalidLogLine(string value)
        {
            FluentActions.Invoking(() => _testClass.Add(value)).Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanCallGetSince()
        {
            // Arrange
            var index = 1672730239L;

            // Act
            var result = _testClass.GetSince(index);

            // Assert
            throw new NotImplementedException("Create or modify test");
        }
    }
}