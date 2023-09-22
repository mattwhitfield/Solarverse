using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Solarverse.Core.Helper;
using Solarverse.Core.Models;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Solarverse.Client
{
    public class DataHubClient : IDataHubClient
    {
        public DataHubClient(IOptions<ClientConfiguration> configuration, ISolarverseApiClient solarverseApiClient)
        {
            _configuration = configuration;

            var url = _configuration.Value.Url;
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException();
            }

            _solarverseApiClient = solarverseApiClient;
            var reconnectionDelays = Enumerable.Range(1, 100).Select(x => TimeSpan.FromSeconds(Math.Min(10, x / 2.0))).ToArray();
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url.TrimEnd('/') + "/DataHub", options =>
                {
                    options.Headers[Headers.ApiKey] = _configuration.Value.ApiKey.ToString();
                })
                .WithAutomaticReconnect(reconnectionDelays)
                .Build();

            _hubConnection.Reconnecting += _hubConnection_Reconnecting;
            _hubConnection.Reconnected += _hubConnection_Reconnected;
            _hubConnection.Closed += _hubConnection_Closed;
        }

        public event EventHandler<EventArgs>? ConnectedChanged;

        private Task _hubConnection_Closed(Exception? arg)
        {
            ConnectedChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        private async Task _hubConnection_Reconnected(string? arg)
        {
            ConnectedChanged?.Invoke(this, EventArgs.Empty);

            await UpdateTimeSeries();
            await UpdateMemoryLog();
            await UpdateCurrentState();
        }

        private Task _hubConnection_Reconnecting(Exception? arg)
        {
            ConnectedChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        private HubConnection _hubConnection;
        private readonly IOptions<ClientConfiguration> _configuration;
        private readonly ISolarverseApiClient _solarverseApiClient;
        private IDisposable? _timeSeriesUpdated;
        private IDisposable? _memoryLogUpdated;
        private IDisposable? _currentStateUpdated;

        public bool IsConnected => (_hubConnection?.State ?? HubConnectionState.Disconnected) == HubConnectionState.Connected;

        public bool IsConnecting => (_hubConnection?.State ?? HubConnectionState.Disconnected) == HubConnectionState.Connecting;

        public Task CloseConnection()
        {
            _timeSeriesUpdated?.Dispose();
            _timeSeriesUpdated = null;
            _memoryLogUpdated?.Dispose();
            _memoryLogUpdated = null;
            _currentStateUpdated?.Dispose();
            _currentStateUpdated = null;
            return _hubConnection.StopAsync();
        }

        public async Task OpenConnection()
        {
            var startTask = _hubConnection.StartAsync();
            ConnectedChanged?.Invoke(this, EventArgs.Empty);
            await startTask;
            _timeSeriesUpdated = _hubConnection.On(DataHubMethods.TimeSeriesUpdated, () => UpdateTimeSeries());
            _memoryLogUpdated = _hubConnection.On(DataHubMethods.MemoryLogUpdated, () => UpdateMemoryLog());
            _currentStateUpdated = _hubConnection.On(DataHubMethods.CurrentStateUpdated, () => UpdateCurrentState());
            ConnectedChanged?.Invoke(this, EventArgs.Empty);
        }

        private Task UpdateTimeSeries()
        {
            return _solarverseApiClient.UpdateTimeSeries();
        }

        private Task UpdateMemoryLog()
        {
            return _solarverseApiClient.UpdateMemoryLog();
        }

        private Task UpdateCurrentState()
        {
            return _solarverseApiClient.UpdateCurrentState();
        }
    }
}
