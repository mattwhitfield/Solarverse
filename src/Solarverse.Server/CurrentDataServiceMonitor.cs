using Microsoft.AspNetCore.SignalR;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Models;
using System.Transactions;

namespace Solarverse.Server
{
    public class CurrentDataServiceMonitor : BackgroundService
    {
        private IHubContext<DataHub> _hubContext;
        bool _timeSeriesUpdated;
        bool _memoryLogUpdated;
        bool _currentStateUpdated;

        public CurrentDataServiceMonitor(ICurrentDataService currentDataService, IMemoryLog memoryLog, IHubContext<DataHub> hubContext)
        {
            _hubContext = hubContext;
            currentDataService.TimeSeriesUpdated += CurrentDataService_TimeSeriesUpdated;
            currentDataService.CurrentStateUpdated += CurrentDataService_CurrentStateUpdated;
            memoryLog.LogUpdated += MemoryLog_LogUpdated;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_timeSeriesUpdated)
                {
                    await _hubContext.Clients.All.SendAsync(DataHubMethods.TimeSeriesUpdated);
                    _timeSeriesUpdated = false;
                }

                if (_memoryLogUpdated)
                {
                    await _hubContext.Clients.All.SendAsync(DataHubMethods.MemoryLogUpdated);
                    _memoryLogUpdated = false;
                }

                if (_currentStateUpdated)
                {
                    await _hubContext.Clients.All.SendAsync(DataHubMethods.CurrentStateUpdated);
                    _currentStateUpdated = false;
                }

                try
                {
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException) 
                { }
            }
        }

        private void CurrentDataService_CurrentStateUpdated(object? sender, EventArgs e)
        {
            _currentStateUpdated = true;
        }

        private void MemoryLog_LogUpdated(object? sender, EventArgs e)
        {
            _memoryLogUpdated = true;
        }

        private void CurrentDataService_TimeSeriesUpdated(object? sender, EventArgs e)
        {
            _timeSeriesUpdated = true;
        }
    }
}
