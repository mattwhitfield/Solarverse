using Microsoft.AspNetCore.Mvc;
using Solarverse.Core.Data;

namespace Solarverse.Server
{
    [Route("api/timeSeries")]
    [ApiController]
    public class TimeSeriesController : ControllerBase
    {
        private readonly ICurrentDataService _currentDataService;

        public TimeSeriesController(ICurrentDataService currentDataService)
        {
            _currentDataService = currentDataService;
        }

        [HttpGet]
        public IEnumerable<TimeSeriesPoint> Get()
        {
            return _currentDataService.TimeSeries;
        }
    }
}
