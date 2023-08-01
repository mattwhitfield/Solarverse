using Microsoft.AspNetCore.Mvc;
using Solarverse.Core.Helper;
using Solarverse.Core.Models;

namespace Solarverse.Server.Controllers
{
    [Route("api/log")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly IMemoryLog _memoryLog;

        public LogController(IMemoryLog memoryLog)
        {
            _memoryLog = memoryLog;
        }

        [HttpGet]
        [Route("since/{index}")]
        public IEnumerable<MemoryLogEntry> Get(long index)
        {
            return _memoryLog.GetSince(index);
        }
    }
}
