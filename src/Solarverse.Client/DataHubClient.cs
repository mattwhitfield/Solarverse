using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Solarverse.Core.Models;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Solarverse.Client
{
    public class DataHubClient : IDataHubClient
    {
        public DataHubClient(IOptions<ClientConfiguration> configuration, ITimeSeriesRetriever timeSeriesRetriever)
        {
            _configuration = configuration;

            var url = _configuration.Value.Url;
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException();
            }

            _timeSeriesRetriever = timeSeriesRetriever;
            var reconnectionDelays = Enumerable.Range(1, 20).Select(x => TimeSpan.FromSeconds(x)).ToArray();
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url.TrimEnd('/') + "/DataHub")
                .WithAutomaticReconnect(reconnectionDelays)
                .Build();
        }

        private HubConnection _hubConnection;
        private readonly IOptions<ClientConfiguration> _configuration;
        private readonly ITimeSeriesRetriever _timeSeriesRetriever;
        private IDisposable? _timeSeriesUpdated;

        public bool IsConnected => (_hubConnection?.State ?? HubConnectionState.Disconnected) == HubConnectionState.Connected;

        public Task CloseConnection()
        {
            _timeSeriesUpdated?.Dispose();
            _timeSeriesUpdated = null;
            return _hubConnection.StopAsync();
        }

        public async Task OpenConnection()
        {
            await _hubConnection.StartAsync();
            _timeSeriesUpdated = _hubConnection.On(DataHubMethods.TimeSeriesUpdated, () => UpdateTimeSeries());
        }

        private Task UpdateTimeSeries()
        {
            return _timeSeriesRetriever.UpdateTimeSeries();
        }
    }
}
