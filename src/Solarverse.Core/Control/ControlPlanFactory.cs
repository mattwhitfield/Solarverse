﻿using Microsoft.Extensions.Logging;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;

namespace Solarverse.Core.Control
{
    public class ControlPlanFactory : IControlPlanFactory
    {
        private readonly ICurrentDataService _currentDataService;
        private readonly ILogger<ControlPlanFactory> _logger;
        private readonly ICurrentTimeProvider _currentTimeProvider;

        public ControlPlanFactory(ICurrentDataService currentDataService, ILogger<ControlPlanFactory> logger, ICurrentTimeProvider currentTimeProvider)
        {
            _currentDataService = currentDataService;
            _logger = logger;
            _currentTimeProvider = currentTimeProvider;
        }

        public void CreatePlan()
        {
            var unsetCount = _currentDataService.TimeSeries.Where(x =>
                x.IncomingRate.HasValue &&
                x.Target == TargetType.Unset).Count();

            _logger.LogInformation($"{unsetCount} points have an incoming rate and no target.");

            if (unsetCount >= 36)
            {
                _logger.LogInformation($"Setting discharge targets.");

                // happns after tariffs become available, so we look at solar forecast, predicted consumption and tariff rates
                var firstTime = _currentTimeProvider.CurrentPeriodStartUtc;
                SetDischargeTargets(firstTime);
                SetEVChargePeriods(firstTime);
            }

            // happens after every current state update - here we check if the battery charge is on track and if we need another 1/2 hour charge period
            if (_currentDataService.TimeSeries.Where(x => x.IncomingRate.HasValue && x.IsFuture(_currentTimeProvider)).Any())
            {
                using var updateLock = _currentDataService.LockForUpdate();

                _currentDataService.TimeSeries.Where(x => x.IsFuture(_currentTimeProvider)).Each(x => x.ControlAction = null);
                _currentDataService.TimeSeries.Where(x => x.IsFuture(_currentTimeProvider) && x.ShouldDischarge() && x.RequiredPowerKwh > 0).Each(x => x.ControlAction = ControlAction.Discharge);

                CreatePlanForDischargeTargets();
            }
        }

        private void SetDischargeTargets(DateTime firstTime)
        {
            _currentDataService.TimeSeries.Set(x => x.RequiredBatteryPowerKwh = null);

            _logger.LogInformation($"Setting discharge targets firstTime = {firstTime}");

            var tariffRates = _currentDataService.TimeSeries
                .GetSeries(x => x.IncomingRate)
                .Where(x => x.Time >= firstTime)
                .ToList();

            var count = tariffRates.Count;
            var ratesByGroup = tariffRates.GroupBy(x => x.Value).OrderByDescending(x => x.Key).ToList();
            _logger.LogInformation($"{count} tariff rate periods in {ratesByGroup.Count} groups");

            var dischargeTimesSet = 0;
            foreach (var rate in ratesByGroup)
            {
                _logger.LogInformation($"Targeting rate group with rate {rate.Key:N2} and {rate.Count()} entries");
                foreach (var point in rate)
                {
                    _currentDataService.TimeSeries.Set(point.Time, x =>
                    {
                        _logger.LogInformation($"Targeting discharge for {point.Time}");
                        x.ControlAction = ControlAction.Discharge;
                        x.Target = TargetType.TariffBasedDischargeRequired;
                    });
                    dischargeTimesSet++;
                }

                if (dischargeTimesSet >= count / 2)
                {
                    break;
                }
            }

            foreach (var point in _currentDataService.TimeSeries.Where(x => x.Target == TargetType.Unset && x.IncomingRate.HasValue))
            {
                point.Target = TargetType.NoDischargeRequired;
            }
        }

        private void SetEVChargePeriods(DateTime firstTime)
        {
            var evChargePeriods = _currentDataService.GetPointsForEVCharging(_logger, firstTime);
            foreach (var point in evChargePeriods)
            {
                point.ShouldChargeEV = true;
            }
        }

        private void CreatePlanForDischargeTargets()
        {
            // now we want to set the 'required battery power' for the periods that we want to discharge
            var forecastPoints = _currentDataService.GetForecastTimeSeries(_logger);
            if (!forecastPoints.Any())
            {
                return;
            }

            _currentDataService.RecalculateForecast();

            SetRequiredPower(forecastPoints);

            // now we make a first pass on the discharge points - find any shortfalls
            // and try to fill them in from points that don't have a current control action
            RunPass("First pass", forecastPoints, x => !x.ControlAction.HasValue);

            // Export when the price is less than zero in an upcoming slot and we have spare capacity
            ExoortWhenPriceIsLessThanZero(forecastPoints);

            // now we make a second pass - the point of this pass is to turn any discharge
            // slots to charge if we have a shortfall somewhere
            RunPass("Second pass", forecastPoints, x => x.ControlAction != ControlAction.Charge);

            // now, make sure that we have all of the top priority points covered
            CoverTopPriorityPoints(forecastPoints);

            // now make sure that the covered discharge periods are catered for - if more expensive
            // periods aren't fully covered, we sacrifice charge for less expensive periods
            PrioritizeDischargeTargetsByPrice(forecastPoints);

            // see if we can utilize any stored charge to reduce fully charged export periods
            ReduceFullyChargedExportPeriods(forecastPoints);

            // set the remaining points to hold/discharge
            SetTailPoints(forecastPoints);

            // now run pairing - identify any periods left where we can pair with another
            // period to charge / discharge cost-effectively
            RunPairingPass(forecastPoints);

            _currentDataService.RecalculateForecast();
        }

        private void CoverTopPriorityPoints(ForecastTimeSeries forecastPoints)
        {
            var targetPoints = forecastPoints.Where(x => x.Target == TargetType.TariffBasedDischargeRequired).OrderByDescending(x => x.IncomingRate).ToList();
            for (int i = 0; i < targetPoints.Count / 2; i++)
            {
                var currentPoint = targetPoints[i];
                if (currentPoint.Next != null && currentPoint.Next.ForecastBatteryPercentage <= forecastPoints.Reserve)
                {
                    var priorPoint = forecastPoints.Where(x => x.Time < currentPoint.Time).TakeWhile(x => x.ForecastBatteryPercentage < 100).Where(x => x.ControlAction == ControlAction.Discharge || x.ControlAction == ControlAction.Export).OrderBy(x => x.IncomingRate).FirstOrDefault();
                    if (priorPoint != null)
                    {
                        _logger.LogInformation($"Point at {currentPoint.Time} is a high priority point with no charge - sacrificing point at {priorPoint.Time}");

                        priorPoint.ControlAction = ControlAction.Hold;
                        _currentDataService.RecalculateForecast();
                    }
                }
            }
        }

        private void ReduceFullyChargedExportPeriods(ForecastTimeSeries forecastPoints)
        {
            _logger.LogInformation($"Reducing fully charged export periods");

            var anyUpdated = true;
            while (anyUpdated)
            {
                anyUpdated = false;
                forecastPoints.RunActionOnSolarExcessStarts(period =>
                {
                    _logger.LogInformation($"Point at {period.Point.Time} being considered as the start of fully charged export period");

                    void ProcessPoints(Func<ForecastTimeSeriesPoint, bool> pointAction)
                    {
                        var pointsUsed = new HashSet<DateTime>();

                        ForecastTimeSeriesPoint? periodPoint;
                        while ((periodPoint = period.PriorPoints().Where(x => !pointsUsed.Contains(x.Time)).OrderByDescending(x => x.IncomingRate).FirstOrDefault()) != null)
                        {
                            var continueProcessing = pointAction(periodPoint);
                            if (!continueProcessing)
                            {
                                break;
                            }

                            pointsUsed.Add(periodPoint.Time);
                        }
                    }

                    if (period.Point.ForecastBatteryPercentage >= 100)
                    {
                        ProcessPoints(periodPoint =>
                        {
                            if (periodPoint.ControlAction.HasValue && periodPoint.ControlAction.Value != ControlAction.Hold)
                            {
                                _logger.LogInformation($"Point at {periodPoint.Time} had an existing control action of {periodPoint.ControlAction}");
                                return true;
                            }

                            if (!periodPoint.RequiredPowerKwh.HasValue || periodPoint.RequiredPowerKwh.Value < 0)
                            {
                                _logger.LogInformation($"Point at {periodPoint.Time} had a required power of '{periodPoint.RequiredPowerKwh}'");
                                return true;
                            }

                            periodPoint.ControlAction = ControlAction.Discharge;
                            anyUpdated = true;

                            _logger.LogInformation($"Point at {periodPoint.Time} requires {periodPoint.RequiredPowerKwh:N2} kWh and battery has {periodPoint.ForecastBatteryKwh:N2} kWh, setting to discharge");

                            _currentDataService.RecalculateForecast();

                            return period.Point.ForecastBatteryPercentage >= 100;
                        });
                    }
                });
            }
        }

        private void ExoortWhenPriceIsLessThanZero(ForecastTimeSeries forecastPoints)
        {
            if (!forecastPoints.Any(x => x.IncomingRate < 0))
            {
                return;
            }

            _logger.LogInformation($"Setting export when possible");

            var pointsOrderedByTime = forecastPoints.OrderBy(x => x.Time).ToList();
            var pointsOrderedByOutgoingRateDescending = forecastPoints.OrderByDescending(x => x.OutgoingRate).ToList();

            foreach (var point in pointsOrderedByOutgoingRateDescending) 
            {
                // we want to export when
                // The current export rate is > 0
                // The battery is fully charged for at least 2 consecutive periods ahead
                // There is a period ahead where incoming rate <= 0

                if (point.OutgoingRate <= 0)// || point.IncomingRate < 0)
                {
                    continue;
                }

                // don't set to export if we don't have any power
                if (point.ForecastBatteryPercentage < forecastPoints.Reserve + 1)
                {
                    continue;
                }

                var pointsUntilEmptyOrTargeted = pointsOrderedByTime.Where(x => x.Time > point.Time).TakeWhile(x => x.ForecastBatteryPercentage > forecastPoints.Reserve + 1 && x.Target != TargetType.TariffBasedDischargeRequired).ToList();
                var anyWithFullCharge = pointsUntilEmptyOrTargeted.Any(x => x.ForecastBatteryPercentage >= 100 && x.Next != null && x.Next.ForecastBatteryPercentage >= 100);
                if (!anyWithFullCharge)
                {
                    // if there are no periods ahead with more than 1 fully charged consecutive point, then continue
                    continue;
                }

                var anyAheadWithNegativeImport = pointsUntilEmptyOrTargeted.Any(x => x.Time > point.Time && x.IncomingRate <= 0);
                if (!anyAheadWithNegativeImport)
                {
                    // if there are no periods ahead with negative rate, then iterating forward any more is pointless
                    continue;
                }


                point.ControlAction = ControlAction.Export;
                _logger.LogInformation($"Point at {point.Time} has points ahead that can cater for export, setting to export");

                _currentDataService.RecalculateForecast();
            }
        }

        private void RunPairingPass(ForecastTimeSeries series)
        {
            // For each remaining point, order them by cost. If there is a chance for us to discharge at
            // higher cost and recharge at lower cost, we should do that. This should take into account
            // whether the pair of points being targeted are split by any sections where the battery is
            // already at 100%
            var remainingPoints = series
                .Where(x => x.ControlAction == ControlAction.Hold && 
                            x.IncomingRate.HasValue &&
                            x.IsFuture(_currentTimeProvider))
                .OrderBy(x => x.IncomingRate)
                .ToList();

            foreach (var point in remainingPoints)
            {
                if (point.ControlAction != ControlAction.Hold)
                {
                    continue;
                }

                var passPoints = remainingPoints.Where(x => x.ControlAction == ControlAction.Hold).ToList();
                var availableChargePercent = (100 - point.ForecastBatteryPercentage ?? series.Reserve);
                var periodCharge = Math.Min(availableChargePercent, series.MaxChargeKwhPerPeriod);
                if (periodCharge > 0)
                {
                    if (!RunSinglePointPass(point, passPoints, series.Efficiency, series.Capacity, periodCharge, series.Reserve))
                    {
                        return;
                    }
                }
            }
        }

        private static void SetTailPoints(ForecastTimeSeries series)
        {
            // now set all remaining points to discharge if there is excess power,
            // or hold if there is not
            var pointWithControlActionPassed = false;
            foreach (var point in series)
            {
                if (!point.IncomingRate.HasValue)
                {
                    continue;
                }

                if (!point.ControlAction.HasValue)
                {
                    if (pointWithControlActionPassed)
                    {
                        point.ControlAction = point.ExcessPowerKwh.HasValue && point.ExcessPowerKwh.Value > 0 ?
                            ControlAction.Discharge : ControlAction.Hold;
                    }
                    else
                    {
                        // at the end of the plan, we just set discharge
                        point.ControlAction = ControlAction.Discharge;
                    }
                }
                else
                {
                    pointWithControlActionPassed = true;
                }    
            }
        }

        private void SetRequiredPower(IEnumerable<ForecastTimeSeriesPoint> forecastPoints)
        {
            var lastPointPower = 0d;
            foreach (var point in forecastPoints)
            {
                if (point.ControlAction == ControlAction.Discharge || point.ShouldDischarge())
                {
                    // todo - need to limit RequiredPowerKwh to maximum charge - because if there is excess - we need to limit that excess to what we can capture in the battery
                    var pointPower = Math.Max(lastPointPower + (point.RequiredPowerKwh ?? 0), 0);
                    point.RequiredBatteryPowerKwh = lastPointPower = pointPower;
                    _logger.LogInformation($"Point at {point.Time} has ideal required power of {point.RequiredBatteryPowerKwh:N2} kWh");
                }
                else
                {
                    lastPointPower = 0;
                }

                // todo - this should look at if we have solar excess later, and if it's profitable to export
                // todo - should also prioritize - maybe there are rates later that have better pricing for charging? (i.e. more negative)
                if (point.IncomingRate.HasValue && point.IncomingRate.Value <= 0.0)
                {
                    _logger.LogInformation($"Point at {point.Time} has negative rate, setting to charge");
                    point.ControlAction = ControlAction.Charge;
                }
            }

            _currentDataService.RecalculateForecast();
        }

        private bool RunSinglePointPass(ForecastTimeSeriesPoint potentialChargePoint, List<ForecastTimeSeriesPoint> points, double efficiency, double capacity, double kwhPerPeriod, int reserve)
        {
            var chargeSlotPercent = (kwhPerPeriod / capacity) * 100;

            var eligibleDischargePointsPrior = points
                .Where(x => x.Time < potentialChargePoint.Time)
                .TakeWhile(x => x.ForecastBatteryPercentage > chargeSlotPercent + reserve);

            var eligibleDischargePointsAfter = points
                .Where(x => x.Time > potentialChargePoint.Time)
                .OrderBy(x => x.Time)
                .TakeWhile(x => x.ForecastBatteryPercentage < 100 - chargeSlotPercent);

            var eligibleDischargePoints = eligibleDischargePointsAfter
                .Concat(eligibleDischargePointsPrior)
                .Where(x => !x.ShouldDischarge())
                .Where(x => x.RequiredPowerKwh > 0)
                .OrderByDescending(x => x.IncomingRate)
                .ToList();

            var dischargePoints = new List<ForecastTimeSeriesPoint>();
            var currentTotalKwh = 0.0;
            var totalCost = 0.0;
            var targetMet = false;
            foreach (var point in eligibleDischargePoints)
            {
                var pointPower = point.RequiredPowerKwh ?? 0;

                if (currentTotalKwh + pointPower > kwhPerPeriod * 1.1)
                {
                    targetMet = true;

                    if (dischargePoints.Count == 0)
                    {
                        dischargePoints.Add(point);
                        currentTotalKwh += pointPower;
                        totalCost += pointPower * point.IncomingRate ?? 0;
                    }
                    break;
                }

                dischargePoints.Add(point);
                currentTotalKwh += pointPower;
                totalCost += pointPower * point.IncomingRate ?? 0;
            }

            if (!dischargePoints.Any())
            {
                _logger.LogInformation($"(Pairing) No targetable periods found");
                return false;
            }

            var targetString = string.Join(',', dischargePoints.Select(x => x.Time.ToString("MM/dd HH:mm")));

            if (!targetMet || currentTotalKwh < kwhPerPeriod * 0.8)
            {
                _logger.LogInformation($"(Pairing) Periods at [{targetString}] did not meet point targeting ({currentTotalKwh:N1} < {kwhPerPeriod:N1} * 0.8)");
                return false;
            }

            var totalCostPerKwh = totalCost / currentTotalKwh;
            _logger.LogInformation($"(Pairing) Targeting periods at [{targetString}] with a total cost of {totalCost:N2}, cost per kwh of {totalCostPerKwh:N2} and required power of {currentTotalKwh:N1}");

            var selectedPointCost = potentialChargePoint.IncomingRate;
            if (selectedPointCost / efficiency > totalCostPerKwh)
            {
                _logger.LogInformation($"(Pairing) Period at {potentialChargePoint.Time} is too expensive ({(selectedPointCost / efficiency):N2})");
                return false;
            }

            _logger.LogInformation($"(Pairing) Setting period {potentialChargePoint.Time} to charge to cater for targeted points");
            dischargePoints.Each(x => {
                x.ControlAction = ControlAction.Discharge;
                x.SetTarget(TargetType.PairingDischargeRequired);
            });

            potentialChargePoint.ControlAction = ControlAction.Charge;
            potentialChargePoint.SetTarget(TargetType.PairingChargeRequired);
            _currentDataService.RecalculateForecast();

            return true;
        }

        private void PrioritizeDischargeTargetsByPrice(ForecastTimeSeries points)
        {
            points.RunActionOnAllDischargeStartPeriods("Prioritize", period =>
            {
                if (period.DischargePoints.Count < 2) 
                {
                    return;
                }

                var averageCost = period.DischargePoints.Average(x => x.IncomingRate);

                var pointsAboveAverageCost = period.DischargePoints.Where(x => x.IncomingRate > averageCost).ToList();
                var pointsBelowAverageCost = period.DischargePoints.Where(x => x.IncomingRate <= averageCost).ToList();

                foreach (var targetPoint in pointsAboveAverageCost.OrderByDescending(x => x.IncomingRate))
                {
                    _logger.LogInformation($"(Prioritize) Point at {targetPoint.Time} has forecast of {targetPoint.ForecastBatteryKwh:N1} battery Kwh and required power of {targetPoint.RequiredPowerKwh:N1}");

                    while (targetPoint.ForecastBatteryKwh < targetPoint.RequiredPowerKwh)
                    {
                        var eligibleHoldPoints = pointsBelowAverageCost
                            .Where(x => x.Time < targetPoint.Time)
                            .Where(x => x.ControlAction != ControlAction.Hold && x.ControlAction != ControlAction.Charge)
                            .Where(x => x.IncomingRate < averageCost)
                            .OrderBy(x => x.IncomingRate)
                            .Take(1)
                            .ToList();

                        if (!eligibleHoldPoints.Any())
                        {
                            _logger.LogInformation($"(Prioritize) No eligible points found");
                            break;
                        }

                        var selectedPoint = eligibleHoldPoints[0];
                        _logger.LogInformation($"(Prioritize) Setting period {selectedPoint.Time} to hold");
                        selectedPoint.ControlAction = ControlAction.Hold;

                        _currentDataService.RecalculateForecast();
                    }
                }

            });
        }

        private void RunPass(string passName, ForecastTimeSeries series, Func<ForecastTimeSeriesPoint, bool> selector)
        {
            series.RunActionOnDischargeStartPeriodsThatNeedMoreCharge(passName, period =>
            {
                var dischargePoint = period.Point;
                var dischargePointPercent = period.PointPercentRequired;

                if (series.MaxChargeKwhPerPeriod <= 0)
                {
                    return;
                }

                // bucketize the discharge points, so we can work out if each charge period is cost effective
                var buckets = Bucketizer.Bucketize(period.DischargePoints, series.MaxChargeKwhPerPeriod);

                while (true)
                {
                    var shortfallPercent = dischargePointPercent - (dischargePoint.ForecastBatteryPercentage ?? 0);
                    var shortfallKwh = series.Capacity * shortfallPercent / 100;

                    if (shortfallKwh <= 0.01)
                    {
                        break;
                    }

                    _logger.LogInformation($"({passName}) Point at {dischargePoint.Time} has forecast of {dischargePoint.ForecastBatteryPercentage:N1}% battery, charge required - shortfall of {shortfallPercent:N1}%, {shortfallKwh:N2} kWh");

                    var eligibleChargePoints = series
                        .Where(x => x.Time < dischargePoint.Time)
                        .TakeWhile(x => x.ForecastBatteryPercentage < 100)
                        .Where(selector)
                        .Where(x => !x.ShouldDischarge())
                        .OrderBy(x => x.IncomingRate)
                        .Take(1)
                        .ToList();

                    if (!eligibleChargePoints.Any() || !buckets.Any())
                    {
                        _logger.LogInformation($"({passName}) No eligible points found");
                        break;
                    }

                    // check if the point we're transforming has a rate low enough to make 
                    // it worth charging
                    var selectedPoint = eligibleChargePoints[0];
                    if (buckets.TakeSlot(selectedPoint.IncomingRate, series.Efficiency))
                    {
                        _logger.LogInformation($"({passName}) Setting period {selectedPoint.Time} to charge");
                        selectedPoint.ControlAction = ControlAction.Charge;

                        var chargeLeft = series.MaxChargeKwhPerPeriod - (shortfallKwh - 0.5);
                        while (chargeLeft > 0)
                        {
                            _logger.LogInformation($"({passName}) {chargeLeft:N2} kWh left to fill");

                            var potentialDischargePoints =
                                series.Where(x => x.IsFuture(_currentTimeProvider) && (!x.ControlAction.HasValue || x.ControlAction == ControlAction.Hold) && x.RequiredPowerKwh < chargeLeft && x.RequiredPowerKwh > 0 && x.IncomingRate.HasValue).ToList();

                            var eligibleDischargePointsPrior = potentialDischargePoints
                                .Where(x => x.Time < selectedPoint.Time)
                                .TakeWhile(x => x.ForecastBatteryKwh > x.RequiredPowerKwh);

                            var eligibleDischargePointsAfter = potentialDischargePoints
                                .Where(x => x.Time > selectedPoint.Time)
                                .OrderBy(x => x.Time)
                                .TakeWhile(x => x.ForecastBatteryPercentage < 100);

                            var potentialDischargePoint = eligibleDischargePointsPrior
                                .Concat(eligibleDischargePointsAfter)
                                .OrderByDescending(x => x.IncomingRate)
                                .Where(x => x.IncomingRate * series.Efficiency > selectedPoint.IncomingRate)
                                .FirstOrDefault();

                            if (potentialDischargePoint == null)
                            {
                                _logger.LogInformation($"({passName}) No eligible points left for filling");
                                break;
                            }

                            _logger.LogInformation($"({passName}) Setting period {potentialDischargePoint.Time} to discharge for filling");
                            potentialDischargePoint.ControlAction = ControlAction.Discharge;
                            chargeLeft -= potentialDischargePoint.RequiredPowerKwh ?? 0;
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"({passName}) Period at {selectedPoint.Time} is too expensive to charge");

                        // if we have already passed the point of it being worthwhile charging
                        // then there's no point continuing;
                        break;
                    }

                    _currentDataService.RecalculateForecast();
                }
            });
        }
    }
}
