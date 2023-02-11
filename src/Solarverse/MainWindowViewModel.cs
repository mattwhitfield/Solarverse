using Solarverse.Core.Control;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Solarverse
{
    public class MainWindowViewModel
    {
        private readonly IControlLoop _controlLoop;
        private readonly ICurrentDataService _currentDataService;
        private readonly IUpdateHandler _updateHandler;
        private readonly IMemoryLog _memoryLog;
        private readonly CancellationTokenSource _controlLoopCancellation;

        private Task? _controlLoopTask;

        public MainWindowViewModel(IControlLoop controlLoop, ICurrentDataService currentDataService, IUpdateHandler updateHandler, IMemoryLog memoryLog)
        {
            _controlLoop = controlLoop;
            _currentDataService = currentDataService;
            _updateHandler = updateHandler;
            _memoryLog = memoryLog;
            _controlLoopCancellation = new CancellationTokenSource();
            _currentDataService.TimeSeriesUpdated += CurrentDataService_TimeSeriesUpdated;
            _currentDataService.CurrentStateUpdated += CurrentDataService_CurrentStateUpdated;
            _memoryLog.LogUpdated += MemoryLog_LogUpdated;
        }

        private void CurrentDataService_CurrentStateUpdated(object? sender, EventArgs e)
        {
            _updateHandler.UpdateCurrentState(_currentDataService.CurrentState);
        }

        long _lastLog = -1;

        private void MemoryLog_LogUpdated(object? sender, EventArgs e)
        {
            var lines = _memoryLog.GetSince(_lastLog).ToList();
            if (lines.Any())
            {
                _lastLog = lines.Max(x => x.Index);
            }

            _updateHandler.AddLogLines(lines);
        }

        private void CurrentDataService_TimeSeriesUpdated(object? sender, EventArgs e)
        {
            _updateHandler.UpdateTimeSeries(_currentDataService.TimeSeries);
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
