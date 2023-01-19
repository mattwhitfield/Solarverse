using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ScottPlot;
using Solarverse.Core.Control;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration.GivEnergy.Models;
using Solarverse.Core.Integration.Octopus.Models;
using Solarverse.Core.Integration.Solcast.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Solarverse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ITimeSeriesHandler
    {
        ServiceProvider _serviceProvider;
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            var collection = ServiceCollectionFactory.Create();
            collection.AddSingleton<ITimeSeriesHandler>(this);
            collection.AddSingleton<MainWindowViewModel>();
            _serviceProvider = collection.BuildServiceProvider();

            DataContext = _viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            WpfPlot1.AxesChanged += WpfPlot1_AxesChanged;
            WpfPlot2.AxesChanged += WpfPlot2_AxesChanged; 
            
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            _viewModel.Start();
            
            //var date = new DateTime(2023, 1, 13, 0, 0, 0, DateTimeKind.Utc);

            //List<NormalizedConsumption> previousConsumption = new[]
            //{
            //    JsonConvert.DeserializeObject<ConsumptionHistory>(File.ReadAllText("C:\\Stuff\\Solarverse\\GivEnergyConsumption20221215.json")),
            //    JsonConvert.DeserializeObject<ConsumptionHistory>(File.ReadAllText("C:\\Stuff\\Solarverse\\GivEnergyConsumption20221222.json")),
            //    JsonConvert.DeserializeObject<ConsumptionHistory>(File.ReadAllText("C:\\Stuff\\Solarverse\\GivEnergyConsumption20221229.json")),
            //    JsonConvert.DeserializeObject<ConsumptionHistory>(File.ReadAllText("C:\\Stuff\\Solarverse\\GivEnergyConsumption20230105.json")),
            //}.Where(x => x != null).Cast<ConsumptionHistory>().Select(x => new NormalizedConsumption(x)).Where(x => x.IsValid).ToList();
            //List<NormalizedConsumption> previousConsumptionNext = new[]
            //{
            //    JsonConvert.DeserializeObject<ConsumptionHistory>(File.ReadAllText("C:\\Stuff\\Solarverse\\GivEnergyConsumption20221216.json")),
            //    JsonConvert.DeserializeObject<ConsumptionHistory>(File.ReadAllText("C:\\Stuff\\Solarverse\\GivEnergyConsumption20221223.json")),
            //    JsonConvert.DeserializeObject<ConsumptionHistory>(File.ReadAllText("C:\\Stuff\\Solarverse\\GivEnergyConsumption20221230.json")),
            //    JsonConvert.DeserializeObject<ConsumptionHistory>(File.ReadAllText("C:\\Stuff\\Solarverse\\GivEnergyConsumption20230106.json")),
            //}.Where(x => x != null).Cast<ConsumptionHistory>().Select(x => new NormalizedConsumption(x)).Where(x => x.IsValid).ToList();

            //var solarForecast = JsonConvert.DeserializeObject<ForecastSet>(File.ReadAllText("C:\\Stuff\\Solarverse\\SolcastForecast.json"));
            //var agileRates = JsonConvert.DeserializeObject<AgileRates>(File.ReadAllText("C:\\Stuff\\Solarverse\\OctopusRates.json"));

            //var cons = new NormalizedConsumption(previousConsumption, date);
            //var consNext = new NormalizedConsumption(previousConsumptionNext, date.AddDays(1));
            //var fore = new NormalizedForecast(solarForecast);

            //var timeSeries = new TimeSeries();
            //if (cons.IsValid)
            //{
            //    timeSeries.AddPointsFrom(cons.DataPoints, x => x.Time, x => x.Consumption, (val, pt) => pt.ConsumptionForecastKwh = val);
            //}

            //if (consNext.IsValid)
            //{
            //    timeSeries.AddPointsFrom(consNext.DataPoints, x => x.Time, x => x.Consumption, (val, pt) => pt.ConsumptionForecastKwh = val);
            //}

            //if (fore.IsValid)
            //{
            //    timeSeries.AddPointsFrom(fore.DataPoints, x => x.Time, x => x.PVEstimate, (val, pt) => pt.PVForecastKwh = val);
            //}

            //if (agileRates?.Rates != null)
            //{
            //    timeSeries.AddPointsFrom(agileRates.Rates, x => x.ValidFrom, x => x.Value, (val, pt) => pt.IncomingRate = val);
            //}
        }

        bool _changing1;
        bool _changing2;

        private void WpfPlot1_AxesChanged(object? sender, EventArgs e)
        {
            if (_changing2)
            {
                return;
            }
            _changing1 = true;
            WpfPlot2.Plot.MatchAxis(WpfPlot1.Plot, horizontal: true, vertical: false);
            WpfPlot2.Plot.MatchLayout(WpfPlot1.Plot, horizontal: true, vertical: false);
            WpfPlot2.Refresh();
            _changing1 = false;
        }

        private void WpfPlot2_AxesChanged(object? sender, EventArgs e)
        {
            if (_changing1)
            {
                return;
            }
            _changing2 = true;
            WpfPlot1.Plot.MatchAxis(WpfPlot2.Plot, horizontal: true, vertical: false);
            WpfPlot1.Plot.MatchLayout(WpfPlot2.Plot, horizontal: true, vertical: false);
            WpfPlot1.Refresh();
            _changing2 = false;
        }

        private void AddPlot(Plot plot, TimeSeries timeSeries, Func<TimeSeriesPoint, double?> value)
        {
            var renderedSeries = timeSeries.GetSeries(value);
            var dataX = renderedSeries.Select(x => x.Time.ToOADate()).ToArray();
            var dataY = renderedSeries.Select(x => x.Value ?? double.NaN).ToArray();
            var scatter = plot.AddScatter(dataX, dataY);
            scatter.OnNaN = ScottPlot.Plottable.ScatterPlot.NanBehavior.Gap;
            scatter.MarkerShape = MarkerShape.none;
        }

        public void UpdateTimeSeries(TimeSeries series)
        {
            WpfPlot1.Plot.Clear();
            WpfPlot2.Plot.Clear();
            WpfPlot1.Plot.YAxis.LockLimits(false);
            WpfPlot2.Plot.YAxis.LockLimits(false);

            AddPlot(WpfPlot1.Plot, series, x => x.ActualConsumptionKwh);
            AddPlot(WpfPlot1.Plot, series, x => x.ConsumptionForecastKwh);
            AddPlot(WpfPlot1.Plot, series, x => x.PVForecastKwh);
            AddPlot(WpfPlot1.Plot, series, x => x.ExcessPowerKwh);

            AddPlot(WpfPlot2.Plot, series, x => x.IncomingRate);
            AddPlot(WpfPlot2.Plot, series, x => x.OutgoingRate);
            AddPlot(WpfPlot2.Plot, series, x => x.CostWithoutStorage);

            WpfPlot1.Plot.XAxis.DateTimeFormat(true);
            WpfPlot2.Plot.XAxis.DateTimeFormat(true);

            WpfPlot1.Refresh();
            WpfPlot2.Refresh();

            WpfPlot1.Plot.YAxis.LockLimits(true);
            WpfPlot2.Plot.YAxis.LockLimits(true);
        }

        bool closing = false;
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (closing)
            {
                return;
            }
            closing = true;

            // TODO - show a 'closing' UI

            e.Cancel = true;

            await _viewModel.Stop();
            Close();
        }
    }
}
