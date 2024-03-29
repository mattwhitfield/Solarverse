﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Solarverse.Core.Control;
using Solarverse.Core.Data;
using Solarverse.Core.Data.Prediction;
using Solarverse.Core.Integration;
using Solarverse.Core.Integration.ForecastSolar;
using Solarverse.Core.Integration.GivEnergy;
using Solarverse.Core.Integration.Octopus;
using Solarverse.Core.Integration.Solcast;

namespace Solarverse.Core.Helper
{
    public static class ServiceCollectionFactory
    {
        public static IServiceCollection CreateForWindows()
        {
            var collection = new ServiceCollection();

            var localData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Solarverse");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(Path.Combine(localData, "logs", "log.txt") , rollingInterval: RollingInterval.Day)
                .CreateLogger();

            collection.AddLogging(builder =>
            {
                builder.AddSerilog(dispose: true);
                builder.Services.TryAddEnumerable(
                    ServiceDescriptor.Singleton<ILoggerProvider, MemoryLoggerProvider>());
            });

            collection.AddTransient<IConfigurationProvider, DefaultConfigurationProvider>();
            collection.AddTransient<ICachePathProvider, WindowsCachePathProvider>();

            collection.AddSolarverse();

            return collection;
        }

        public static void AddSolarverse(this IServiceCollection collection)
        {
            collection.AddSingleton<IDataStore, FileDataStore>();
            collection.AddTransient<IControlLoop, ControlLoop>();
            collection.AddSingleton<IInverterClient, GivEnergyClient>();
            collection.AddSingleton<IEVChargerClient, GivEnergyClient>();
            collection.AddSingleton<ISolarForecastClient, SolcastClient>();
            //collection.AddSingleton<ISolarForecastClient, ForecastSolarClient>();
            collection.AddSingleton<IEnergySupplierClient, OctopusClient>();
            collection.AddSingleton<ICurrentDataService, CurrentDataService>();
            collection.AddTransient<IControlPlanExecutor, ControlPlanExecutor>();
            collection.AddTransient<IPredictionFactory, PredictionFactory>();
            collection.AddTransient<IControlPlanFactory, ControlPlanFactory>();
            collection.AddTransient<IIntegrationProvider, IntegrationProvider>();
            collection.AddSingleton<IMemoryLog, MemoryLog>();
            collection.AddTransient<ICurrentTimeProvider, CurrentTimeProvider>();
        }
    }
}
