namespace Solarverse.Core.Models
{
    public class MemoryLogEntry
    {
        public MemoryLogEntry(long index, string message)
        {
            Index = index;
            Message = message;
        }

        public long Index { get; }

        public string Message { get; }
    }
}
