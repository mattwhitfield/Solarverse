namespace Solarverse.Core.Tests.Models
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Models;
    using Xunit;

    public class MemoryLogEntryTests
    {
        private MemoryLogEntry _testClass;
        private long _index;
        private string _message;

        public MemoryLogEntryTests()
        {
            _index = 551712136L;
            _message = "TestValue1179598433";
            _testClass = new MemoryLogEntry(_index, _message);
        }

        [Fact]
        public void CanConstruct()
        {
            // Act
            var instance = new MemoryLogEntry(_index, _message);

            // Assert
            instance.Should().NotBeNull();
        }

        [Fact]
        public void IndexIsInitializedCorrectly()
        {
            _testClass.Index.Should().Be(_index);
        }

        [Fact]
        public void MessageIsInitializedCorrectly()
        {
            _testClass.Message.Should().Be(_message);
        }
    }
}