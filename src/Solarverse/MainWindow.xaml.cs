using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ScottPlot;
using ScottPlot.Plottable;
using Solarverse.Core.Control;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Integration.GivEnergy.Models;
using Solarverse.Core.Integration.Octopus.Models;
using Solarverse.Core.Integration.Solcast.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Solarverse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ITimeSeriesHandler
    {
        ServiceProvider _serviceProvider;
        private MainWindowViewModel _viewModel;

        Crosshair _crosshair1, _crosshair2;
        bool _inPlot1, _inPlot2;

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

        private void SetCrosshairX(double x)
        {
            var xRounded = Math.Round(x * 48) / 48;

            _crosshair1.X = xRounded;
            _crosshair2.X = xRounded;

            var visible = _inPlot1 || _inPlot2;
            _crosshair2.IsVisible = _crosshair1.IsVisible = visible;

            var date = DateTime.FromOADate(xRounded);
            if (visible && _currentSeries.TryGetDataPointFor(date, out var point))
            {
                if (point.ActualConsumptionKwh.HasValue)
                {
                    ConsumptionLabel.Text = point.ActualConsumptionKwh.Value.ToString("N1") + " kWh (actual)";
                }
                else if (point.ConsumptionForecastKwh.HasValue)
                {
                    ConsumptionLabel.Text = point.ConsumptionForecastKwh.Value.ToString("N1") + " kWh (forecast)";
                }
                else
                {
                    ConsumptionLabel.Text = "-";
                }

                if (point.PVActualKwh.HasValue)
                {
                    SolarLabel.Text = point.PVActualKwh.Value.ToString("N1") + " kWh (actual)";
                }
                else if (point.PVForecastKwh.HasValue)
                {
                    SolarLabel.Text = point.PVForecastKwh.Value.ToString("N1") + " kWh (forecast)";
                }
                else
                {
                    SolarLabel.Text = "-";
                }

                ExcessPowerLabel.Text = point.ExcessPowerKwh.HasValue ? point.ExcessPowerKwh.Value.ToString("N1") + " kWh (forecast)" : "-";
                IncomingRateLabel.Text = point.IncomingRate.HasValue ? point.IncomingRate.Value.ToString("N2") + "p" : "-";
                OutgoingRateLabel.Text = point.OutgoingRate.HasValue ? point.OutgoingRate.Value.ToString("N2") + "p" : "-";
                ProjectedCostLabel.Text = point.CostWithoutStorage.HasValue ? point.CostWithoutStorage.Value.ToString("N1") + "p" : "-";
                DateTimeLabel.Text = date.ToString("dd/MM HH:mm");
            }
            else
            {
                DateTimeLabel.Text = "-";
                ConsumptionLabel.Text = "-";
                SolarLabel.Text = "-";
                ExcessPowerLabel.Text = "-";
                IncomingRateLabel.Text = "-";
                OutgoingRateLabel.Text = "-";
                ProjectedCostLabel.Text = "-";
            }

            WpfPlot1.Refresh();
            WpfPlot2.Refresh();
        }

        private void wpfPlot1_OnMouseMove(object sender, MouseEventArgs e)
        {
            (double coordinateX, _) = WpfPlot1.GetMouseCoordinates();
            SetCrosshairX(coordinateX);
        }

        private void wpfPlot2_OnMouseMove(object sender, MouseEventArgs e)
        {
            (double coordinateX, _) = WpfPlot2.GetMouseCoordinates();
            SetCrosshairX(coordinateX);
        }

        private void wpfPlot1_MouseEnter(object sender, MouseEventArgs e)
        {
            _inPlot1 = true;
        }

        private void wpfPlot1_MouseLeave(object sender, MouseEventArgs e)
        {
            _inPlot1 = false;
            SetCrosshairX(0);
        }

        private void wpfPlot2_MouseEnter(object sender, MouseEventArgs e)
        {
            _inPlot2 = true;
        }

        private void wpfPlot2_MouseLeave(object sender, MouseEventArgs e)
        {
            _inPlot2 = false;
            SetCrosshairX(0);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            _viewModel.Start();
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

        private void AddPlot(Plot plot, TimeSeries timeSeries, Func<TimeSeriesPoint, double?> value, Color color, bool isActual)
        {
            AddPlot(plot, timeSeries, value, color, isActual, _ => true);
        }

        private void AddPlot(Plot plot, TimeSeries timeSeries, Func<TimeSeriesPoint, double?> value, Color color, bool isActual, Func<DateTime, bool> timeFilter)
        {
            var renderedSeries = timeSeries.GetSeries(value);
            var dataX = renderedSeries.Where(x => timeFilter(x.Time)).Select(x => x.Time.ToOADate()).ToArray();
            var dataY = renderedSeries.Where(x => timeFilter(x.Time)).Select(x => x.Value ?? double.NaN).ToArray();
            var scatter = plot.AddScatter(dataX, dataY);
            scatter.Color = color;
            scatter.LineStyle = isActual ? LineStyle.Solid : LineStyle.Dash;
            scatter.OnNaN = ScottPlot.Plottable.ScatterPlot.NanBehavior.Gap;
            scatter.MarkerShape = MarkerShape.none;
        }

        private Crosshair AddAndConfigureCrosshair(Plot plot)
        {
            var crosshair = plot.AddCrosshair(0, 0);
            crosshair.VerticalLine.PositionLabel = false;
            crosshair.VerticalLine.Color = Color.Black;
            crosshair.VerticalLine.LineStyle = LineStyle.Solid;
            crosshair.IsVisible = false;
            crosshair.HorizontalLine.IsVisible = false;
            return crosshair;
        }

        public void UpdateTimeSeries(TimeSeries series)
        {
            _currentSeries = series;

            WpfPlot1.Plot.Clear();
            WpfPlot2.Plot.Clear();

            _crosshair1 = AddAndConfigureCrosshair(WpfPlot1.Plot);
            _crosshair2 = AddAndConfigureCrosshair(WpfPlot2.Plot);

            WpfPlot1.Plot.YAxis.LockLimits(false);
            WpfPlot2.Plot.YAxis.LockLimits(false);
            
            WpfPlot1.Plot.XAxis.Ticks(false);
            WpfPlot1.Plot.XAxis.Line(false);
            WpfPlot1.Plot.YAxis2.Line(false);
            WpfPlot1.Plot.XAxis2.Line(false);

            //WpfPlot2.Plot.XAxis.Ticks(false);
            //WpfPlot2.Plot.XAxis.Line(false);
            WpfPlot2.Plot.YAxis2.Line(false);
            WpfPlot2.Plot.XAxis2.Line(false);

            var maxTimeWithPvActual = series.GetSeries(x => x.PVActualKwh).Where(x => x.Value.HasValue).Select(x => x.Time).DefaultIfEmpty(DateTime.MinValue).Max();
            AddPlot(WpfPlot1.Plot, series, x => x.ActualConsumptionKwh, Color.DarkBlue, true);
            AddPlot(WpfPlot1.Plot, series, x => x.ConsumptionForecastKwh, Color.DarkBlue, false);
            AddPlot(WpfPlot1.Plot, series, x => x.PVActualKwh, Color.DarkGoldenrod, true);
            AddPlot(WpfPlot1.Plot, series, x => x.PVForecastKwh, Color.DarkGoldenrod, false, x => x >= maxTimeWithPvActual);
            AddPlot(WpfPlot1.Plot, series, x => x.ExcessPowerKwh, Color.DarkGreen, false);

            AddPlot(WpfPlot2.Plot, series, x => x.IncomingRate, Color.Purple, true);
            AddPlot(WpfPlot2.Plot, series, x => x.OutgoingRate, Color.OrangeRed, true);
            AddPlot(WpfPlot2.Plot, series, x => x.CostWithoutStorage, Color.DarkRed, false);

            WpfPlot1.Plot.XAxis.DateTimeFormat(true);
            WpfPlot2.Plot.XAxis.DateTimeFormat(true);

            WpfPlot1.Refresh();
            WpfPlot2.Refresh();

            WpfPlot1.Plot.Layout(left: 50, right: 0, bottom: 0, top: 0);
            WpfPlot1.Plot.XAxis.Layout(minimumSize: 0, maximumSize: 0);
            WpfPlot1.Plot.XAxis2.Layout(minimumSize: 0, maximumSize: 0);
            WpfPlot2.Plot.Layout(left: 50, right: 0, bottom: 0, top: 0);
            //WpfPlot2.Plot.XAxis.Layout(minimumSize: 0, maximumSize: 0);
            WpfPlot2.Plot.XAxis2.Layout(minimumSize: 0, maximumSize: 0);

            WpfPlot1.Plot.YAxis.LockLimits(true);
            WpfPlot2.Plot.YAxis.LockLimits(true);
        }

        bool closing = false;
        private TimeSeries _currentSeries;

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
