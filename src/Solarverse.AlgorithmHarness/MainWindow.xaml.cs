using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Solarverse.Core.Control;
using Solarverse.Core.Data;
using Solarverse.Core.Data.Prediction;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration.GivEnergy;
using Solarverse.Core.Integration.Octopus;
using Solarverse.Core.Integration.Solcast;
using Solarverse.Core.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Serilog.Core;
using System.IO;

namespace Solarverse.AlgorithmHarness
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ICurrentTimeProvider
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly MainWindowViewModel _viewModel;

        private string _fileName;
        private DateTime _current;
        private DateTime _min;
        private DateTime _max;

        public DateTime LocalNow => _current.ToLocalTime();

        public DateTime UtcNow => _current;

        public DateTime CurrentPeriodStartUtc => _current;

        public TimeSpan Offset => TimeSpan.Zero;

        public MainWindow()
        {
            InitializeComponent();

            var configuration = StaticConfigurationProvider.Configuration;

            Graph.ShowDevelopmentDetail = configuration.TestMode;
            Graph.BatteryCapacityKwh = configuration.Battery.CapacityKwh ?? 5.2;
            Graph.ShowCurrentTime = false;

            var collection = CreateForWindows();
            collection.AddSingleton<IUpdateHandler>(Graph);
            collection.AddSingleton<MainWindowViewModel>();
            _serviceProvider = collection.BuildServiceProvider();

            DataContext = _viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        }

        public IServiceCollection CreateForWindows()
        {
            var collection = new ServiceCollection();

            collection.AddLogging(builder =>
            {
                builder.Services.TryAddEnumerable(
                    ServiceDescriptor.Singleton<ILoggerProvider, MemoryLoggerProvider>());
            });

            collection.AddTransient<IConfigurationProvider, DefaultConfigurationProvider>();
            collection.AddTransient<ICachePathProvider, WindowsCachePathProvider>();

            collection.AddSingleton<ICurrentDataService, CurrentDataService>();
            collection.AddTransient<IControlPlanFactory, ControlPlanFactory>();
            collection.AddSingleton<ICurrentTimeProvider>(this);
            collection.AddSingleton<IMemoryLog, MemoryLog>();

            return collection;
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            // TODO - pick file name
            _fileName = "C:\\stuff\\Solarverse\\Snapshots\\need_to_sort_plunge_prices.json";
            _fileName = "C:\\stuff\\Solarverse\\Snapshots\\20240824152136.json";

            var series = ReadFile();
            _min = series.Min(x => x.Time);
            _max = series.Max(x => x.Time);

            _current = series.Where(x => x.ActualBatteryPercentage.HasValue).Max(x => x.Time);

            //_current = new DateTime(2024, 1, 24, 23, 0, 0, DateTimeKind.Utc);

            Modify(x => x);
        }

        private List<TimeSeriesPoint> ReadFile()
        {
            return JsonConvert.DeserializeObject<List<TimeSeriesPoint>>(File.ReadAllText(_fileName)) ?? throw new InvalidOperationException();            
        }

        private void Modify(Func<DateTime, DateTime> modify)
        {
            var newDate = modify(_current);
            if (newDate >= _min && newDate <= _max)
            {
                _current = newDate;
            }

            var series = ReadFile();
            series.Each(x =>
            {
                x.ControlAction = null;
                x.Target = TargetType.Unset;
            });

            var nextCutoff = new Period(TimeSpan.FromDays(1), TimeSpan.FromHours(16));
            var last4Pm = nextCutoff.GetLast(_current);
            var timeUntil = last4Pm.AddDays(1).AddHours(7);

            series.Where(x => x.Time >= timeUntil).Each(p =>
            {
                p.IncomingRate = null;
                p.OutgoingRate = null;
                p.ForecastBatteryPercentage = null;
                p.ActualBatteryPercentage = null;
            });

            series.Where(x => x.Time >= _current).Each(p =>
            {
                p.ForecastBatteryPercentage ??= p.ActualBatteryPercentage;
                p.ForecastConsumptionKwh ??= p.ActualConsumptionKwh;
                p.ForecastSolarKwh ??= p.ActualSolarKwh;

                p.ActualBatteryPercentage = null;
                p.ActualConsumptionKwh = null;
                p.ActualSolarKwh = null;
            });

            series.Where(x => x.Time < _current).Each(p =>
            {
                p.ActualBatteryPercentage ??= p.ForecastBatteryPercentage;
                p.ActualConsumptionKwh ??= p.ForecastConsumptionKwh;
                p.ActualSolarKwh ??= p.ForecastSolarKwh;
                p.ForecastBatteryPercentage = null;
                p.ForecastConsumptionKwh = null;
                p.ForecastSolarKwh = null;
            });

            _serviceProvider.GetRequiredService<ICurrentDataService>().InitializeTimeSeries(series);

            var factory = _serviceProvider.GetRequiredService<IControlPlanFactory>();
            Graph.ClearLogLines();
            factory.CreatePlan();
        }

        private void NextDay(object sender, RoutedEventArgs e)
        {
            Modify(x => x.AddDays(1));
        }

        private void Next(object sender, RoutedEventArgs e)
        {
            Modify(x => x.AddHours(1));
        }

        private void Previous(object sender, RoutedEventArgs e)
        {
            Modify(x => x.AddHours(-1));
        }

        private void PreviousDay(object sender, RoutedEventArgs e)
        {
            Modify(x => x.AddDays(-1));
        }

        public DateTime ToLocalTime(DateTime utcTime)
        {
            return utcTime.ToLocalTime();
        }

        public TimeSpan ToLocalTime(TimeSpan utcTime)
        {
            return DateTime.UtcNow.Add(utcTime).ToLocalTime().TimeOfDay;
        }

        public DateTime FromLocalTime(DateTime localTime)
        {
            return localTime;
        }
    }
}