using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using ScottPlot;
using ScottPlot.Plottable;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Solarverse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ITimeSeriesHandler
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly MainWindowViewModel _viewModel;

        Crosshair? _crosshair1, _crosshair2, _crosshair3;
        bool _inPlot1, _inPlot2, _inPlot3;
        bool _changing1, _changing2, _changing3;

        bool _closing = false;
        private TimeSeries? _currentSeries;

        public MainWindow()
        {
            InitializeComponent();

            WpfPlot1.Plot.XAxis.Ticks(false);
            WpfPlot1.Plot.XAxis.Line(false);
            WpfPlot1.Plot.YAxis2.Line(false);
            WpfPlot1.Plot.XAxis2.Line(false);

            WpfPlot2.Plot.XAxis.Ticks(false);
            WpfPlot2.Plot.XAxis.Line(false);
            WpfPlot2.Plot.YAxis2.Line(false);
            WpfPlot2.Plot.XAxis2.Line(false);

            WpfPlot3.Plot.YAxis2.Line(false);
            WpfPlot3.Plot.XAxis2.Line(false);

            WpfPlot1.Plot.XAxis.DateTimeFormat(true);
            WpfPlot2.Plot.XAxis.DateTimeFormat(true);
            WpfPlot3.Plot.XAxis.DateTimeFormat(true);

            WpfPlot1.Plot.Layout(left: 50, right: 0, bottom: 0, top: 0);
            WpfPlot1.Plot.XAxis.Layout(minimumSize: 0, maximumSize: 0);
            WpfPlot1.Plot.XAxis2.Layout(minimumSize: 0, maximumSize: 0);
            WpfPlot2.Plot.Layout(left: 50, right: 0, bottom: 0, top: 0);
            WpfPlot2.Plot.XAxis.Layout(minimumSize: 0, maximumSize: 0);
            WpfPlot2.Plot.XAxis2.Layout(minimumSize: 0, maximumSize: 0);
            WpfPlot3.Plot.Layout(left: 50, right: 0, bottom: 0, top: 0);
            WpfPlot3.Plot.XAxis2.Layout(minimumSize: 0, maximumSize: 0);

            var collection = ServiceCollectionFactory.Create();
            collection.AddSingleton<ITimeSeriesHandler>(this);
            collection.AddSingleton<MainWindowViewModel>();
            _serviceProvider = collection.BuildServiceProvider();

            DataContext = _viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            WpfPlot1.AxesChanged += WpfPlot1_AxesChanged;
            WpfPlot2.AxesChanged += WpfPlot2_AxesChanged;
            WpfPlot3.AxesChanged += WpfPlot3_AxesChanged;

            Loaded += OnLoaded;
        }

        private void SetCrosshairX(double x)
        {
            if (_crosshair1 == null || _crosshair2 == null || _crosshair3 == null || _currentSeries == null)
            {
                return;
            }

            var xRounded = Math.Round(x * 48) / 48;

            _crosshair1.X = xRounded;
            _crosshair2.X = xRounded;
            _crosshair3.X = xRounded;

            var visible = _inPlot1 || _inPlot2 || _inPlot3;
            _crosshair3.IsVisible = _crosshair2.IsVisible = _crosshair1.IsVisible = visible;

            var date = DateTime.FromOADate(xRounded);
            if (visible && _currentSeries.TryGetDataPointFor(date, out var point))
            {
                if (point.ActualConsumptionKwh.HasValue)
                {
                    ConsumptionLabel.Text = point.ActualConsumptionKwh.Value.ToString("N1") + " kWh (actual)";
                }
                else if (point.ForecastConsumptionKwh.HasValue)
                {
                    ConsumptionLabel.Text = point.ForecastConsumptionKwh.Value.ToString("N1") + " kWh (forecast)";
                }
                else
                {
                    ConsumptionLabel.Text = "-";
                }

                if (point.ActualSolarKwh.HasValue)
                {
                    SolarLabel.Text = point.ActualSolarKwh.Value.ToString("N1") + " kWh (actual)";
                }
                else if (point.ForecastSolarKwh.HasValue)
                {
                    SolarLabel.Text = point.ForecastSolarKwh.Value.ToString("N1") + " kWh (forecast)";
                }
                else
                {
                    SolarLabel.Text = "-";
                }

                if (point.ActualBatteryPercentage.HasValue)
                {
                    ProjectedBatteryPercentLabel.Text = point.ActualBatteryPercentage.Value.ToString("N0") + "% (actual)";
                }
                else if (point.ForecastBatteryPercentage.HasValue)
                {
                    ProjectedBatteryPercentLabel.Text = point.ForecastBatteryPercentage.Value.ToString("N0") + "% (forecast)";
                }
                else
                {
                    ProjectedBatteryPercentLabel.Text = "-";
                }

                ExcessPowerLabel.Text = point.ExcessPowerKwh.HasValue ? point.ExcessPowerKwh.Value.ToString("N1") + " kWh (forecast)" : "-";
                IncomingRateLabel.Text = point.IncomingRate.HasValue ? point.IncomingRate.Value.ToString("N2") + "p" : "-";
                OutgoingRateLabel.Text = point.OutgoingRate.HasValue ? point.OutgoingRate.Value.ToString("N2") + "p" : "-";
                ProjectedCostLabel.Text = point.CostWithoutStorage.HasValue ? point.CostWithoutStorage.Value.ToString("N1") + "p" : "-";
                DateTimeLabel.Text = date.ToString("dd/MM HH:mm");
                ScheduledActionLabel.Text = point.ControlAction.HasValue ? point.ControlAction.Value.ToString() : "-";
                if (point.ControlAction.HasValue)
                {
                    switch (point.ControlAction.Value)
                    {
                        case ControlAction.Charge:
                            ScheduledActionPath.Data = Shapes.UpTriangle;
                            ScheduledActionPath.Fill = System.Windows.Media.Brushes.Salmon;
                            break;
                        case ControlAction.Discharge:
                            ScheduledActionPath.Data = Shapes.DownTriangle;
                            ScheduledActionPath.Fill = System.Windows.Media.Brushes.MediumSeaGreen;
                            break;
                        case ControlAction.Export:
                            ScheduledActionPath.Data = Shapes.DoubleDownTriangle;
                            ScheduledActionPath.Fill = System.Windows.Media.Brushes.DodgerBlue;
                            break;
                        case ControlAction.Hold:
                            ScheduledActionPath.Data = Shapes.Circle;
                            ScheduledActionPath.Fill = System.Windows.Media.Brushes.Silver;
                            break;
                    }
                }
                else
                {
                    ScheduledActionPath.Data = Shapes.Circle;
                    ScheduledActionPath.Fill = System.Windows.Media.Brushes.Black;
                }
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
                ProjectedBatteryPercentLabel.Text = "-";
                ScheduledActionLabel.Text = "-";
                ScheduledActionPath.Data = Shapes.Circle;
                ScheduledActionPath.Fill = System.Windows.Media.Brushes.Black;
            }

            WpfPlot1.Refresh();
            WpfPlot2.Refresh();
            WpfPlot3.Refresh();
        }

        private void WpfPlot1_OnMouseMove(object sender, MouseEventArgs e)
        {
            (double coordinateX, _) = WpfPlot1.GetMouseCoordinates();
            SetCrosshairX(coordinateX);
        }

        private void WpfPlot1_MouseEnter(object sender, MouseEventArgs e)
        {
            _inPlot1 = true;
        }

        private void WpfPlot1_MouseLeave(object sender, MouseEventArgs e)
        {
            _inPlot1 = false;
            SetCrosshairX(0);
        }

        private void WpfPlot2_OnMouseMove(object sender, MouseEventArgs e)
        {
            (double coordinateX, _) = WpfPlot2.GetMouseCoordinates();
            SetCrosshairX(coordinateX);
        }

        private void WpfPlot2_MouseEnter(object sender, MouseEventArgs e)
        {
            _inPlot2 = true;
        }

        private void WpfPlot2_MouseLeave(object sender, MouseEventArgs e)
        {
            _inPlot2 = false;
            SetCrosshairX(0);
        }

        private void WpfPlot3_OnMouseMove(object sender, MouseEventArgs e)
        {
            (double coordinateX, _) = WpfPlot3.GetMouseCoordinates();
            SetCrosshairX(coordinateX);
        }

        private void WpfPlot3_MouseEnter(object sender, MouseEventArgs e)
        {
            _inPlot3 = true;
        }

        private void WpfPlot3_MouseLeave(object sender, MouseEventArgs e)
        {
            _inPlot3 = false;
            SetCrosshairX(0);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            _viewModel.Start();
        }

        private void WpfPlot1_AxesChanged(object? sender, EventArgs e)
        {
            if (_changing2 || _changing3)
            {
                return;
            }
            _changing1 = true;
            WpfPlot2.Plot.MatchAxis(WpfPlot1.Plot, horizontal: true, vertical: false);
            WpfPlot2.Plot.MatchLayout(WpfPlot1.Plot, horizontal: true, vertical: false);
            WpfPlot2.Refresh();
            WpfPlot3.Plot.MatchAxis(WpfPlot1.Plot, horizontal: true, vertical: false);
            WpfPlot3.Plot.MatchLayout(WpfPlot1.Plot, horizontal: true, vertical: false);
            WpfPlot3.Refresh();
            _changing1 = false;
        }

        private void WpfPlot2_AxesChanged(object? sender, EventArgs e)
        {
            if (_changing1 || _changing3)
            {
                return;
            }
            _changing2 = true;
            WpfPlot1.Plot.MatchAxis(WpfPlot2.Plot, horizontal: true, vertical: false);
            WpfPlot1.Plot.MatchLayout(WpfPlot2.Plot, horizontal: true, vertical: false);
            WpfPlot1.Refresh();
            WpfPlot3.Plot.MatchAxis(WpfPlot2.Plot, horizontal: true, vertical: false);
            WpfPlot3.Plot.MatchLayout(WpfPlot2.Plot, horizontal: true, vertical: false);
            WpfPlot3.Refresh();
            _changing2 = false;
        }

        private void WpfPlot3_AxesChanged(object? sender, EventArgs e)
        {
            if (_changing1 || _changing2)
            {
                return;
            }
            _changing3 = true;
            WpfPlot1.Plot.MatchAxis(WpfPlot3.Plot, horizontal: true, vertical: false);
            WpfPlot1.Plot.MatchLayout(WpfPlot3.Plot, horizontal: true, vertical: false);
            WpfPlot1.Refresh();
            WpfPlot2.Plot.MatchAxis(WpfPlot3.Plot, horizontal: true, vertical: false);
            WpfPlot2.Plot.MatchLayout(WpfPlot3.Plot, horizontal: true, vertical: false);
            WpfPlot2.Refresh();
            _changing3 = false;
        }

        private static void AddPlot(Plot plot, TimeSeries timeSeries, Func<TimeSeriesPoint, double?> value, Color color, bool isActual)
        {
            AddPlot(plot, timeSeries, value, color, isActual, _ => true);
        }

        private static ScatterPlot AddPlot(Plot plot, TimeSeries timeSeries, Func<TimeSeriesPoint, double?> value, Color color, bool isActual, Func<DateTime, bool> timeFilter)
        {
            var renderedSeries = timeSeries.GetNullableSeries(value);
            var dataX = renderedSeries.Where(x => timeFilter(x.Time)).Select(x => x.Time.ToOADate()).ToArray();
            var dataY = renderedSeries.Where(x => timeFilter(x.Time)).Select(x => x.Value ?? double.NaN).ToArray();
            var scatter = plot.AddScatter(dataX, dataY);
            scatter.Color = color;
            scatter.LineStyle = isActual ? LineStyle.Solid : LineStyle.Dash;
            scatter.OnNaN = ScottPlot.Plottable.ScatterPlot.NanBehavior.Gap;
            scatter.MarkerShape = MarkerShape.none;
            return scatter;
        }

        private static Crosshair AddAndConfigureCrosshair(Plot plot)
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
            WpfPlot3.Plot.Clear();

            _crosshair1 = AddAndConfigureCrosshair(WpfPlot1.Plot);
            _crosshair2 = AddAndConfigureCrosshair(WpfPlot2.Plot);
            _crosshair3 = AddAndConfigureCrosshair(WpfPlot3.Plot);

            WpfPlot1.Plot.YAxis.LockLimits(false);
            WpfPlot2.Plot.YAxis.LockLimits(false);
            WpfPlot3.Plot.YAxis.LockLimits(false);

            var maxTimeWithPvActual = series.GetNullableSeries(x => x.ActualSolarKwh).Where(x => x.Value.HasValue).Select(x => x.Time).DefaultIfEmpty(DateTime.MinValue).Max();
            AddPlot(WpfPlot1.Plot, series, x => x.ActualConsumptionKwh, Color.DarkBlue, true);
            AddPlot(WpfPlot1.Plot, series, x => x.ForecastConsumptionKwh, Color.DarkBlue, false);
            AddPlot(WpfPlot1.Plot, series, x => x.ActualSolarKwh, Color.DarkGoldenrod, true);
            AddPlot(WpfPlot1.Plot, series, x => x.ForecastSolarKwh, Color.DarkGoldenrod, false, x => x >= maxTimeWithPvActual);
            AddPlot(WpfPlot1.Plot, series, x => x.ExcessPowerKwh, Color.Green, false);

            AddPlot(WpfPlot2.Plot, series, x => x.IncomingRate, Color.Purple, true);
            AddPlot(WpfPlot2.Plot, series, x => x.OutgoingRate, Color.OrangeRed, true);
            AddPlot(WpfPlot2.Plot, series, x => x.CostWithoutStorage, Color.Red, false);

            AddPlot(WpfPlot3.Plot, series, x => x.ActualBatteryPercentage, Color.Black, true);
            AddPlot(WpfPlot3.Plot, series, x => x.ForecastBatteryPercentage, Color.Black, false, x => x >= maxTimeWithPvActual);
            AddPlot(WpfPlot3.Plot, series, x => (x.RequiredBatteryPowerKwh / ConfigurationProvider.Configuration.Battery.CapacityKwh) * 100, Color.DarkBlue, true, x => x >= maxTimeWithPvActual).MarkerShape = MarkerShape.filledCircle;
            AddPlot(WpfPlot3.Plot, series, x => (x.MaxCarryForwardChargeKwh / ConfigurationProvider.Configuration.Battery.CapacityKwh) * 100, Color.MediumPurple, true, x => x >= maxTimeWithPvActual).MarkerShape = MarkerShape.filledCircle;

            foreach (var point in series)
            {
                var action = point.ControlAction;
                if (action.HasValue)
                {
                    switch (action.Value)
                    {
                        case ControlAction.Charge:
                            WpfPlot3.Plot.AddMarker(point.Time.ToOADate(), 50, MarkerShape.filledTriangleUp, 8, Color.Salmon);
                            break;
                        case ControlAction.Hold:
                            WpfPlot3.Plot.AddMarker(point.Time.ToOADate(), 50, MarkerShape.filledCircle, 8, Color.Silver);
                            break;
                        case ControlAction.Discharge:
                            WpfPlot3.Plot.AddMarker(point.Time.ToOADate(), 50, MarkerShape.filledTriangleDown, 8, Color.MediumSeaGreen);
                            break;
                        case ControlAction.Export:
                            WpfPlot3.Plot.AddMarker(point.Time.ToOADate(), 49.25, MarkerShape.filledTriangleDown, 8, Color.DodgerBlue);
                            WpfPlot3.Plot.AddMarker(point.Time.ToOADate(), 50.75, MarkerShape.filledTriangleDown, 8, Color.DodgerBlue);
                            break;
                    }
                }

                if (point.IsDischargeTarget)
                {
                    WpfPlot3.Plot.AddMarker(point.Time.ToOADate(), 55, MarkerShape.asterisk, 4, Color.FromArgb(64, Color.Red));
                }
            }

            WpfPlot1.Refresh();
            WpfPlot2.Refresh();
            WpfPlot3.Refresh();

            WpfPlot1.Plot.YAxis.LockLimits(true);
            WpfPlot2.Plot.YAxis.LockLimits(true);
            WpfPlot3.Plot.YAxis.LockLimits(true);
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closing)
            {
                return;
            }
            _closing = true;

            // TODO - show a 'closing' UI

            e.Cancel = true;

            await _viewModel.Stop();
            Close();
        }
    }
}
