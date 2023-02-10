using ScottPlot;
using ScottPlot.Plottable;
using Solarverse.Core.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Solarverse.UI.Core
{
    public partial class GraphControl : UserControl, ITimeSeriesHandler
    {
        HSpan? _span1, _span2, _span3;

        readonly bool[] _inPlot = new bool[3];
        readonly bool[] _changing = new bool[3];
        readonly WpfPlot[] _plots;

        private TimeSeries? _currentSeries;

        public bool ShowDevelopmentDetail { get; set; }

        public double BatteryCapacityKwh { get; set; } = 5.2;

        public GraphControl()
        {
            InitializeComponent();

            SetUpPlots();
            _plots = new[] { WpfPlot1, WpfPlot2, WpfPlot3 };
        }

        private void SetUpPlots()
        {
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

            WpfPlot1.AxesChanged += WpfPlot_AxesChanged;
            WpfPlot2.AxesChanged += WpfPlot_AxesChanged;
            WpfPlot3.AxesChanged += WpfPlot_AxesChanged;
        }

        private void SetCrosshairX(double x)
        {
            if (_span1 == null || _span2 == null || _span3 == null || _currentSeries == null)
            {
                return;
            }

            var xRounded = Math.Floor(x * 48) / 48;
            var xNextRounded = (Math.Floor(x * 48) + 1) / 48;

            _span3.X1 = _span2.X1 = _span1.X1 = xRounded;
            _span3.X2 = _span2.X2 = _span1.X2 = xNextRounded;

            var visible = _inPlot.Any(x => x);
            _span3.IsVisible = _span2.IsVisible = _span1.IsVisible = visible;

            var date = DateTime.FromOADate(xRounded);
            if (visible && _currentSeries.TryGetDataPointFor(date, out var point))
            {
                _currentSeries.TryGetDataPointFor(date.AddMinutes(30), out var nextPoint);

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

                string GetBatteryDescription(TimeSeriesPoint sourcePoint)
                {
                    if (sourcePoint.ActualBatteryPercentage.HasValue)
                    {
                        return sourcePoint.ActualBatteryPercentage.Value.ToString("N0") + "% (actual)";
                    }
                    else if (sourcePoint.ForecastBatteryPercentage.HasValue)
                    {
                        return sourcePoint.ForecastBatteryPercentage.Value.ToString("N0") + "% (forecast)";
                    }
                    else
                    {
                        return "-";
                    }
                }

                if (point.ExcessPowerKwh.HasValue)
                {
                    if (point.ExcessPowerKwh.Value >= 0)
                    {
                        ExcessPowerCircle.Fill = System.Windows.Media.Brushes.Green;
                        ExcessPowerTitleLabel.Text = "Excess Power:";
                        ExcessPowerLabel.Text = point.ExcessPowerKwh.Value.ToString("N1") + " kWh (forecast)";
                    }
                    else
                    {
                        ExcessPowerCircle.Fill = System.Windows.Media.Brushes.Red;
                        ExcessPowerTitleLabel.Text = "Required Power:";
                        ExcessPowerLabel.Text = (point.ExcessPowerKwh.Value * -1).ToString("N1") + " kWh (forecast)";
                    }
                }
                else
                {
                    ExcessPowerCircle.Fill = System.Windows.Media.Brushes.Black;
                    ExcessPowerTitleLabel.Text = "Excess Power:";
                    ExcessPowerLabel.Text = "-";
                }
                
                IncomingRateLabel.Text = point.IncomingRate.HasValue ? point.IncomingRate.Value.ToString("N2") + "p" : "-";
                OutgoingRateLabel.Text = point.OutgoingRate.HasValue ? point.OutgoingRate.Value.ToString("N2") + "p" : "-";
                if (nextPoint != null)
                {
                    ProjectedBatteryPercentLabel.Text = GetBatteryDescription(point) + " -> " + GetBatteryDescription(nextPoint);
                    DateTimeLabel.Text = date.ToString("dd/MM HH:mm") + " -> " + nextPoint.Time.ToString("dd/MM HH:mm");
                }
                else
                {
                    ProjectedBatteryPercentLabel.Text = GetBatteryDescription(point);
                    DateTimeLabel.Text = date.ToString("dd/MM HH:mm");
                }
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
                ProjectedBatteryPercentLabel.Text = "-";
                ScheduledActionLabel.Text = "-";
                ScheduledActionPath.Data = Shapes.Circle;
                ScheduledActionPath.Fill = System.Windows.Media.Brushes.Black;
            }

            WpfPlot1.Refresh();
            WpfPlot2.Refresh();
            WpfPlot3.Refresh();
        }

        private int GetWpfPlotIndex(object? sender)
        {
            for (int i = 0; i < _plots.Length; i++)
            {
                if (ReferenceEquals(_plots[i], sender))
                {
                    return i;
                }
            }

            throw new InvalidOperationException();
        }

        private void WpfPlot_OnMouseMove(object sender, MouseEventArgs e)
        {
            var wpfPlot = (WpfPlot)sender;

            (double coordinateX, _) = wpfPlot.GetMouseCoordinates();
            SetCrosshairX(coordinateX);
        }

        private void WpfPlot_MouseEnter(object sender, MouseEventArgs e)
        {
            _inPlot[GetWpfPlotIndex(sender)] = true;
        }

        private void WpfPlot_MouseLeave(object sender, MouseEventArgs e)
        {
            _inPlot[GetWpfPlotIndex(sender)] = false;
            SetCrosshairX(0);
        }

        private void WpfPlot_AxesChanged(object? sender, EventArgs e)
        {
            if (sender is WpfPlot thisPlot)
            {
                var index = GetWpfPlotIndex(sender);

                for (int i = 0; i < _changing.Length; i++)
                {
                    if (i != index && _changing[i])
                    {
                        return;
                    }
                }

                _changing[index] = true;
                foreach (var other in _plots.Where(x => !ReferenceEquals(x, sender)))
                {
                    other.Plot.MatchAxis(thisPlot.Plot, horizontal: true, vertical: false);
                    other.Plot.MatchLayout(thisPlot.Plot, horizontal: true, vertical: false);
                    other.Refresh();
                }
                _changing[index] = false;
            }
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

        private static ScatterPlot AddSteppedPlot(Plot plot, TimeSeries timeSeries, Func<TimeSeriesPoint, double?> value, Color color, bool isActual)
        {
            return AddSteppedPlot(plot, timeSeries, value, color, isActual, _ => true);
        }

        private static ScatterPlot AddSteppedPlot(Plot plot, TimeSeries timeSeries, Func<TimeSeriesPoint, double?> value, Color color, bool isActual, Func<DateTime, bool> timeFilter)
        {
            var renderedSeries = timeSeries.GetNullableSeries(value);
            var dataX = new List<double>();
            var dataY = new List<double>();

            foreach (var point in renderedSeries.Where(x => timeFilter(x.Time)))
            {
                var time = point.Time.ToOADate();
                var nextTime = point.Time.AddSeconds(1799).ToOADate();
                var y = point.Value ?? double.NaN;

                dataX.Add(time);
                dataX.Add(nextTime);
                dataY.Add(y);
                dataY.Add(y);
            }

            var scatter = plot.AddScatter(dataX.ToArray(), dataY.ToArray());
            scatter.Color = color;
            scatter.LineStyle = isActual ? LineStyle.Solid : LineStyle.Dash;
            scatter.OnNaN = ScottPlot.Plottable.ScatterPlot.NanBehavior.Gap;
            scatter.MarkerShape = MarkerShape.none;
            return scatter;
        }

        private static HSpan AddAndConfigureHspan(Plot plot)
        {
            var span = plot.AddHorizontalSpan(DateTime.UtcNow.ToOADate(), DateTime.UtcNow.ToOADate(), Color.FromArgb(32, Color.Black));
            span.IsVisible = false;
            return span;
        }

        private static void AddFillBetween(Plot plot, TimeSeries series, Func<TimeSeriesPoint, double?> value1, Func<TimeSeriesPoint, double?> value2, Func<double, bool> include, Func<DateTime, bool> timeFilter, Color color)
        {
            var times = new List<double>();
            var y1 = new List<double>();
            var y2 = new List<double>();
            foreach (var point in series.Where(x => timeFilter(x.Time)).OrderBy(x => x.Time))
            {
                var val1 = value1(point);
                var val2 = value2(point);
                if (val1.HasValue && val2.HasValue)
                {
                    var diff = val2.Value - val1.Value;
                    if (include(diff))
                    {
                        times.Add(point.Time.ToOADate());
                        y1.Add(val1.Value);
                        y2.Add(val2.Value);
                        times.Add(point.Time.AddSeconds(1799).ToOADate());
                        y1.Add(val1.Value);
                        y2.Add(val2.Value);
                    }
                    else
                    {
                        if (times.Any())
                        {
                            plot.AddFill(times.ToArray(), y1.ToArray(), y2.ToArray(), color);
                        }
                        times.Clear();
                        y1.Clear();
                        y2.Clear();
                    }
                }
            }

            if (times.Any())
            {
                plot.AddFill(times.ToArray(), y1.ToArray(), y2.ToArray(), color);
            }
        }

        public void UpdateTimeSeries(TimeSeries series)
        {
            Dispatcher.BeginInvoke(new Action(() => UpdateTimeSeriesSafe(series)));
        }

        private bool _updating;
        public void UpdateTimeSeriesSafe(TimeSeries series)
        {
            if (_updating)
            {
                return;
            }

            _updating = true;

            _currentSeries = series;

            WpfPlot1.Plot.Clear();
            WpfPlot2.Plot.Clear();
            WpfPlot3.Plot.Clear();

            _span1 = AddAndConfigureHspan(WpfPlot1.Plot);
            _span2 = AddAndConfigureHspan(WpfPlot2.Plot);
            _span3 = AddAndConfigureHspan(WpfPlot3.Plot);

            WpfPlot1.Plot.YAxis.LockLimits(false);
            WpfPlot2.Plot.YAxis.LockLimits(false);
            WpfPlot3.Plot.YAxis.LockLimits(false);

            var maxTimeWithPvActual = series.GetNullableSeries(x => x.ActualSolarKwh).Where(x => x.Value.HasValue).Select(x => x.Time).DefaultIfEmpty(DateTime.MinValue).Max();

            AddFillBetween(WpfPlot1.Plot, series, x => x.ActualConsumptionKwh, x => x.ActualSolarKwh, x => x > 0, _ => true, Color.FromArgb(20, Color.Green));
            AddFillBetween(WpfPlot1.Plot, series, x => x.ForecastConsumptionKwh, x => x.ForecastSolarKwh, x => x > 0, x => x > maxTimeWithPvActual, Color.FromArgb(20, Color.Green));
            AddFillBetween(WpfPlot1.Plot, series, x => x.ActualSolarKwh, x => x.ActualConsumptionKwh, x => x > 0, _ => true, Color.FromArgb(20, Color.Red));
            AddFillBetween(WpfPlot1.Plot, series, x => x.ForecastSolarKwh, x => x.ForecastConsumptionKwh, x => x > 0, x => x > maxTimeWithPvActual, Color.FromArgb(20, Color.Red));

            AddSteppedPlot(WpfPlot1.Plot, series, x => x.ActualConsumptionKwh, Color.DarkBlue, true);
            AddSteppedPlot(WpfPlot1.Plot, series, x => x.ForecastConsumptionKwh, Color.DarkBlue, false, x => x > maxTimeWithPvActual);
            AddSteppedPlot(WpfPlot1.Plot, series, x => x.ActualSolarKwh, Color.DarkGoldenrod, true);
            AddSteppedPlot(WpfPlot1.Plot, series, x => x.ForecastSolarKwh, Color.DarkGoldenrod, false, x => x > maxTimeWithPvActual);

            AddSteppedPlot(WpfPlot2.Plot, series, x => x.IncomingRate, Color.Purple, true);
            AddSteppedPlot(WpfPlot2.Plot, series, x => x.OutgoingRate, Color.OrangeRed, true);

            AddPlot(WpfPlot3.Plot, series, x => x.ActualBatteryPercentage, Color.Black, true);
            AddPlot(WpfPlot3.Plot, series, x => x.ForecastBatteryPercentage, Color.Black, false, x => x > maxTimeWithPvActual);
            if (ShowDevelopmentDetail)
            {
                AddPlot(WpfPlot3.Plot, series, x => (x.RequiredBatteryPowerKwh / BatteryCapacityKwh) * 100, Color.DarkBlue, true, x => x >= maxTimeWithPvActual).MarkerShape = MarkerShape.filledCircle;
                AddPlot(WpfPlot3.Plot, series, x => (x.MaxCarryForwardChargeKwh / BatteryCapacityKwh) * 100, Color.MediumPurple, true, x => x >= maxTimeWithPvActual).MarkerShape = MarkerShape.filledCircle;
            }

            foreach (var point in series)
            {
                var plottableTime = point.Time.AddMinutes(15).ToOADate();

                var action = point.ControlAction;
                if (action.HasValue)
                {
                    switch (action.Value)
                    {
                        case ControlAction.Charge:
                            WpfPlot3.Plot.AddMarker(plottableTime, 50, MarkerShape.filledTriangleUp, 8, Color.Salmon);
                            break;
                        case ControlAction.Hold:
                            WpfPlot3.Plot.AddMarker(plottableTime, 50, MarkerShape.filledCircle, 8, Color.Silver);
                            break;
                        case ControlAction.Discharge:
                            WpfPlot3.Plot.AddMarker(plottableTime, 50, MarkerShape.filledTriangleDown, 8, Color.MediumSeaGreen);
                            break;
                        case ControlAction.Export:
                            WpfPlot3.Plot.AddMarker(plottableTime, 49.25, MarkerShape.filledTriangleDown, 8, Color.DodgerBlue);
                            WpfPlot3.Plot.AddMarker(plottableTime, 50.75, MarkerShape.filledTriangleDown, 8, Color.DodgerBlue);
                            break;
                    }
                }

                if (point.IsDischargeTarget)
                {
                    WpfPlot3.Plot.AddMarker(plottableTime, 55, MarkerShape.asterisk, 4, Color.FromArgb(64, Color.Red));
                }
            }

            WpfPlot1.Refresh();
            WpfPlot2.Refresh();
            WpfPlot3.Refresh();

            WpfPlot1.Plot.YAxis.LockLimits(true);
            WpfPlot2.Plot.YAxis.LockLimits(true);
            WpfPlot3.Plot.YAxis.LockLimits(true);

            _updating = false;
        }
    }
}
