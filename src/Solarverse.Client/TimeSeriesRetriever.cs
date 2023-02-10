using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Solarverse.Core.Data;
using Solarverse.Core.Helper;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Solarverse.Client
{
    internal class TimeSeriesRetriever : ITimeSeriesRetriever
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TimeSeriesRetriever> _logger;
        private readonly IOptions<ClientConfiguration> _configuration;
        private readonly ITimeSeriesHandler _timeSeriesHandler;

        public TimeSeriesRetriever(ILogger<TimeSeriesRetriever> logger, IOptions<ClientConfiguration> configuration, ITimeSeriesHandler timeSeriesHandler)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _configuration = configuration;
            _timeSeriesHandler = timeSeriesHandler;
        }

        public async Task UpdateTimeSeries()
        {
            var url = _configuration.Value.Url + "/api/timeSeries";
            var timeSeries = await _httpClient.Get<IList<TimeSeriesPoint>>(_logger, url);
            _timeSeriesHandler.UpdateTimeSeries(new TimeSeries(timeSeries));
        }
    }
}
