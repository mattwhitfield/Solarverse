using Microsoft.AspNetCore.Mvc;
using Solarverse.Core.Data;
using Solarverse.Core.Models;

namespace Solarverse.Server
{
    [Route("api/currentState")]
    [ApiController]
    public class CurrentStateController : ControllerBase
    {
        private readonly ICurrentDataService _currentDataService;

        public CurrentStateController(ICurrentDataService currentDataService)
        {
            _currentDataService = currentDataService;
        }

        [HttpGet]
        public InverterCurrentState Get()
        {
            return _currentDataService.CurrentState;
        }
    }
}
