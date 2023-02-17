namespace Solarverse.Core.Tests.Helper
{
    using System;
    using System.Linq;
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
        public void CanCallAddAndGetSince()
        {
            _testClass.Add("TestLine1");
            _testClass.Add("TestLine2");
            _testClass.Add("TestLine3");
            _testClass.Add("TestLine4");

            var result = _testClass.GetSince(0).ToList();
            result.Select(x => x.Message).Should().BeEquivalentTo("TestLine1", "TestLine2", "TestLine3", "TestLine4");

            _testClass.Add("TestLine5");
            _testClass.Add("TestLine6");
            _testClass.Add("TestLine7");
            _testClass.Add("TestLine8");

            result = _testClass.GetSince(4).ToList();
            result.Select(x => x.Message).Should().BeEquivalentTo("TestLine5", "TestLine6", "TestLine7", "TestLine8");
        }

        [Fact]
        public void CanCallGetSinceWithoutAdding()
        {
            // Act
            var result = _testClass.GetSince(0);

            // Assert
            result.Should().BeEmpty();
        }
    }
}