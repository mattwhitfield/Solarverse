using Solarverse.Core.Data;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace Solarverse.Client
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IDataHubClient _dataHubClient;
        private readonly IUpdateHandler _updateHandler;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel(IDataHubClient dataHubClient, IUpdateHandler updateHandler)
        {
            _dataHubClient = dataHubClient;
            _updateHandler = updateHandler;
            _dataHubClient.ConnectedChanged += _dataHubClient_ConnectedChanged;
        }

        private void _dataHubClient_ConnectedChanged(object? sender, EventArgs e)
        {
            if (_dataHubClient.IsConnected)
            {
                _updateHandler.SetConnectionState(ConnectionState.RemoteConnected);
            }
            else if (_dataHubClient.IsConnecting)
            {
                _updateHandler.SetConnectionState(ConnectionState.RemoteConnecting);
            }
            else
            {
                _updateHandler.SetConnectionState(ConnectionState.RemoteDisconnected);
            }
        }

        public bool IsConnected => _dataHubClient.IsConnected;

        public async Task Start()
        {
            int attempts = 0;
            TimeSpan delayTime = TimeSpan.FromSeconds(0.75);

            while (true)
            {
                try
                {
                    await _dataHubClient.OpenConnection();
                    return;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (HttpRequestException)
                {
                    attempts++;
                    if (attempts > 50)
                    {
                        throw;
                    }

                    await Task.Delay(delayTime);
                    delayTime *= 1.25;
                }
            }
        }

        public Task Stop()
        {
            return _dataHubClient.CloseConnection();
        }
    }
}
