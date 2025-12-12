using Microsoft.AspNetCore.SignalR;

namespace NgoHuuDuc_2280600725.Hubs
{
    public class OrderHub : Hub
    {
        public async Task SendOrderNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveOrderNotification", message);
        }
    }
}
