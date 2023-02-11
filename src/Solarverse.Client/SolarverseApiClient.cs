using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using Solarverse.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Solarverse.Client
{
    internal class SolarverseApiClient : ISolarverseApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SolarverseApiClient> _logger;
        private readonly IOptions<ClientConfiguration> _configuration;
        private readonly IUpdateHandler _updateHandler;
        private long _lastMemoryLog = -1;

        public SolarverseApiClient(ILogger<SolarverseApiClient> logger, IOptions<ClientConfiguration> configuration, IUpdateHandler updateHandler)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _configuration = configuration;
            _updateHandler = updateHandler;
        }

        public async Task UpdateCurrentState()
        {
            var url = _configuration.Value.Url + "/api/currentState";
            var currentState = await _httpClient.Get<InverterCurrentState>(_logger, url);
            _updateHandler.UpdateCurrentState(currentState);
        }

        public async Task UpdateMemoryLog()
        {
            var url = _configuration.Value.Url + "/api/log/since/" + _lastMemoryLog.ToString();
            var logEntries = await _httpClient.Get<IList<MemoryLogEntry>>(_logger, url);
            if (logEntries.Any())
            {
                _lastMemoryLog = logEntries.Max(x => x.Index);
                _updateHandler.AddLogLines(logEntries);
            }
        }

        public async Task UpdateTimeSeries()
        {
            var url = _configuration.Value.Url + "/api/timeSeries";
            var timeSeries = await _httpClient.Get<IList<TimeSeriesPoint>>(_logger, url);
            _updateHandler.UpdateTimeSeries(new TimeSeries(timeSeries));
        }
    }
}
