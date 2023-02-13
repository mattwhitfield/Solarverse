namespace Solarverse.Core.Tests.Helper
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Solarverse.Core.Helper;
    using Xunit;

    public static class ServiceCollectionFactoryTests
    {
        [Fact]
        public static void CanCallCreate()
        {
            // Act
            var collection = ServiceCollectionFactory.CreateForWindows();

            using (var serviceProvider = collection.BuildServiceProvider())
            {
                foreach (var componentRegistration in collection)
                {
                    var serviceType = componentRegistration.ServiceType;

                    if (serviceType.Assembly.FullName.StartsWith("Solarverse"))
                    {
                        var service = serviceProvider.GetService(serviceType);

                        service.Should().NotBeNull();
                    }
                }
            }
        }
    }
}