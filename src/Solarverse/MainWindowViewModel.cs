using Solarverse.Core.Control;
using Solarverse.Core.Data;
using Solarverse.UI.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Solarverse
{
    public class MainWindowViewModel
    {
        private readonly IControlLoop _controlLoop;
        private readonly ICurrentDataService _currentDataService;
        private readonly ITimeSeriesHandler _timeSeriesHandler;
        private readonly CancellationTokenSource _controlLoopCancellation;

        private Task? _controlLoopTask;

        public MainWindowViewModel(IControlLoop controlLoop, ICurrentDataService currentDataService, ITimeSeriesHandler timeSeriesHandler)
        {
            _controlLoop = controlLoop;
            _currentDataService = currentDataService;
            _timeSeriesHandler = timeSeriesHandler;
            _controlLoopCancellation = new CancellationTokenSource();
            _currentDataService.TimeSeriesUpdated += CurrentDataService_TimeSeriesUpdated;
        }

        private void CurrentDataService_TimeSeriesUpdated(object? sender, EventArgs e)
        {
            _timeSeriesHandler.UpdateTimeSeries(_currentDataService.TimeSeries);
        }

        public void Start()
        {
            _controlLoopTask = _controlLoop.Run(_controlLoopCancellation.Token);
        }

        public async Task Stop()
        {
            if (_controlLoopTask != null)
            {
                _controlLoopCancellation.Cancel();
                await _controlLoopTask;
            }
        }
    }
}
