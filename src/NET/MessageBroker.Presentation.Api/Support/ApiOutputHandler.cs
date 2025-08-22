using MessageBroker.Example.CrossCut.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Presentation.Api.Support;

public class ApiOutputHandler : IExampleOutputHandler
{
    public Task WriteOutputAsync(string message, CancellationToken ct)
    {
        // Optionally log or ignore output in API context
        return Task.CompletedTask;
    }

    public Task RenderMenuAsync(IReadOnlyDictionary<char, MessageBroker.Example.CrossCut.Factories.ExampleDetails> examples, CancellationToken ct)
    {
        // No-op for API context
        return Task.CompletedTask;
    }

    public Task ClearScreenAsync(CancellationToken ct)
    {
        // No-op for API context
        return Task.CompletedTask;
    }
}
