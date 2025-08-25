using Microsoft.AspNetCore.SignalR;

namespace MessageBroker.Presentation.Api.Support;

public class MessageHub : Hub
{
    // Send a message to all connected clients
    public async Task SendMessage(string consumerKey, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", consumerKey, message);
    }
}
