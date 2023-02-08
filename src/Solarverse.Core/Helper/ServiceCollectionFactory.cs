using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Solarverse.Core.Control;
using Solarverse.Core.Data;
using Solarverse.Core.Data.Prediction;
using Solarverse.Core.Integration;
using Solarverse.Core.Integration.GivEnergy;
using Solarverse.Core.Integration.Octopus;
using Solarverse.Core.Integration.Solcast;

namespace Solarverse.Core.Helper
{
    public static class ServiceCollectionFactory
    {
        public static IServiceCollection Create()
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
            });

            collection.AddTransient<IConfigurationProvider, DefaultConfigurationProvider>();
            collection.AddTransient<IControlLoop, ControlLoop>();
            collection.AddSingleton<IInverterClient, GivEnergyClient>();
            collection.AddSingleton<ISolarForecastClient, SolcastClient>();
            collection.AddSingleton<IEnergySupplierClient, OctopusClient>();
            collection.AddSingleton<ICurrentDataService, CurrentDataService>();
            collection.AddSingleton<IDataStore, FileDataStore>();
            collection.AddTransient<IControlPlanExecutor, ControlPlanExecutor>();
            collection.AddTransient<IPredictionFactory, PredictionFactory>();
            collection.AddTransient<IControlPlanFactory, ControlPlanFactory>();
            collection.AddTransient<IIntegrationProvider, IntegrationProvider>();
            return collection;
        }
    }
}
