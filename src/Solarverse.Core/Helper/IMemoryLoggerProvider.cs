namespace Solarverse.Core.Helper
{
    using Microsoft.Extensions.Logging;
    using System.Collections.Concurrent;

    public class MemoryLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, MemoryLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
        private readonly IMemoryLog _memoryLog;

        public MemoryLoggerProvider(IMemoryLog memoryLog)
        {
            _memoryLog = memoryLog;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new MemoryLogger(name, _memoryLog));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
