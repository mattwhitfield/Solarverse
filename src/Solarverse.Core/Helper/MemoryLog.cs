using Solarverse.Core.Models;

namespace Solarverse.Core.Helper
{
    public class MemoryLog : IMemoryLog
    {
        public const int LogLength = 10000;

        List<MemoryLogEntry> _logLines = new List<MemoryLogEntry>();
        long _index;
        private static object LockObject = new object();

        public event EventHandler<EventArgs>? LogUpdated;

        public void Add(string logLine)
        {
            lock (LockObject)
            {
                _logLines.Add(new MemoryLogEntry(++_index, logLine));
                while (_logLines.Count > LogLength)
                {
                    _logLines.RemoveAt(0);
                }
            }

            LogUpdated?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerable<MemoryLogEntry> GetSince(long index)
        {
            // TODO - optimize this
            return _logLines.Where(x => x.Index > index);
        }
    }
}
