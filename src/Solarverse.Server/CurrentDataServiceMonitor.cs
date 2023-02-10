using Microsoft.AspNetCore.SignalR;
using Solarverse.Core.Data;
using Solarverse.Core.Models;

namespace Solarverse.Server
{
    public class CurrentDataServiceMonitor : BackgroundService
    {
        private IHubContext<DataHub> _hubContext;
        bool _timeSeriesUpdated;

        public CurrentDataServiceMonitor(ICurrentDataService currentDataService, IHubContext<DataHub> hubContext)
        {
            _hubContext = hubContext;
            currentDataService.TimeSeriesUpdated += CurrentDataService_TimeSeriesUpdated;
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

                try
                {
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException) 
                { }
            }
        }

        private void CurrentDataService_TimeSeriesUpdated(object? sender, EventArgs e)
        {
            _timeSeriesUpdated = true;
        }
    }
}
