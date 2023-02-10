using Newtonsoft.Json;
using Solarverse.Core.Control;
using Solarverse.Core.Data;
using System;
using System.Net.Http;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;

namespace Solarverse.Client
{
    public class MainWindowViewModel
    {
        private readonly IDataHubClient _dataHubClient;

        public MainWindowViewModel(IDataHubClient dataHubClient)
        {
            _dataHubClient = dataHubClient;
        }

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
