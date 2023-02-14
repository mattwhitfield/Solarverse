namespace Solarverse.Core.Tests.Data
{
    using System;
    using FluentAssertions;
    using Solarverse.Core.Data;
    using Xunit;

    public class ContainerCachePathProviderTests
    {
        private ContainerCachePathProvider _testClass;

        public ContainerCachePathProviderTests()
        {
            _testClass = new ContainerCachePathProvider();
        }

        [Fact]
        public void CanGetCachePath()
        {
            // Assert
            _testClass.CachePath.Should().BeAssignableTo<string>();

            throw new NotImplementedException("Create or modify test");
        }
    }
}