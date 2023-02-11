using Solarverse.Core.Models;

namespace Solarverse.Core.Helper
{
    public interface IMemoryLog
    {
        event EventHandler<EventArgs> LogUpdated;

        void Add(string logLine);

        IEnumerable<MemoryLogEntry> GetSince(long index);
    }
}