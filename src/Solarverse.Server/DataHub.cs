using Microsoft.AspNetCore.SignalR;

namespace Solarverse.Server
{
    public class DataHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
    }
}
