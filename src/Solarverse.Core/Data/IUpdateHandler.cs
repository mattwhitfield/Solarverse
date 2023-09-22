using Solarverse.Core.Models;

namespace Solarverse.Core.Data
{
    public interface IUpdateHandler
    {
        public void UpdateTimeSeries(TimeSeries series);

        public void AddLogLines(IEnumerable<MemoryLogEntry> entries);

        public void UpdateCurrentState(InverterCurrentState currentState);

        public void SetConnectionState(ConnectionState connectionState);
    }
}
