namespace Solarverse.Core.Tests.Data
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Data;
    using Xunit;

    public class WindowsCachePathProviderTests
    {
        private WindowsCachePathProvider _testClass;

        public WindowsCachePathProviderTests()
        {
            _testClass = new WindowsCachePathProvider();
        }

        [Fact]
        public void CanGetCachePath()
        {
            // Assert
            _testClass.CachePath.Should().Be(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }
    }
}