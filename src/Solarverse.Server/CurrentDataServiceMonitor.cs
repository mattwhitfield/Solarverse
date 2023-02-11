using Microsoft.AspNetCore.SignalR;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Models;

namespace Solarverse.Server
{
    public class CurrentDataServiceMonitor : BackgroundService
    {
        private IHubContext<DataHub> _hubContext;
        bool _timeSeriesUpdated;
        bool _memoryLogUpdated;

        public CurrentDataServiceMonitor(ICurrentDataService currentDataService, IMemoryLog memoryLog, IHubContext<DataHub> hubContext)
        {
            _hubContext = hubContext;
            currentDataService.TimeSeriesUpdated += CurrentDataService_TimeSeriesUpdated;
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

                try
                {
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException) 
                { }
            }
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
