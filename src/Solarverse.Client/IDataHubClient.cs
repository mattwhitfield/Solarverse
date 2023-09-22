using System;
using System.Threading.Tasks;

namespace Solarverse.Client
{
    public interface IDataHubClient
    {
        bool IsConnected { get; }

        bool IsConnecting { get; }

        Task CloseConnection();

        Task OpenConnection();

        event EventHandler<EventArgs> ConnectedChanged;
    }
}