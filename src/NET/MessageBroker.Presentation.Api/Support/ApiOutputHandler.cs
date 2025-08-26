using MessageBroker.Example.CrossCut.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MessageBroker.Presentation.Api.Support;

public class ApiOutputHandler : IExampleOutputHandler
{
    private readonly IHubContext<MessageHub> _hubContext;

    public ApiOutputHandler(IHubContext<MessageHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task WriteOutputAsync(string message, CancellationToken ct)
    {
        // Broadcast to all SignalR clients (consumerKey can be empty or generic)
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", "api-consumer", message, ct);
    }

    public Task RenderMenuAsync(IReadOnlyDictionary<char, MessageBroker.Example.CrossCut.Factories.ExampleDetails> examples, CancellationToken ct)
    {
        // No-op for API context
        return Task.CompletedTask;
    }
}
