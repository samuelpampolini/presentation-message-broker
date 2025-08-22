using MessageBroker.Example.CrossCut.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Presentation.Api.Support;

public class ApiInputProvider : IExampleInputProvider
{
    public Task<string> GetInputAsync(string prompt, CancellationToken ct)
    {
        // Always return an empty string or a default value for API context
        return Task.FromResult("");
    }
}
