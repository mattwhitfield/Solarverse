using System.Threading.Tasks;

namespace Solarverse.Client
{
    public interface IDataHubClient
    {
        bool IsConnected { get; }

        Task CloseConnection();
        Task OpenConnection();
    }
}