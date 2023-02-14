namespace Solarverse.Core.Helper
{
    using Microsoft.Extensions.Logging;

    public class MemoryLogger : ILogger
    {
        private readonly string _name;
        private readonly IMemoryLog _memoryLog;

        public MemoryLogger(string name, IMemoryLog memoryLog)
        {
            _name = name.Substring(name.LastIndexOf('.') + 1);
            _memoryLog = memoryLog;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _memoryLog.Add($"[{DateTime.UtcNow:yyyyMMdd HH:mm:ss} {_name}] {formatter(state, exception)}");
        }
    }
}
