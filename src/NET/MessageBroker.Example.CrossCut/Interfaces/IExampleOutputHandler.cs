namespace MessageBroker.Example.CrossCut.Interfaces;

using System.Collections.Generic;
using MessageBroker.Example.CrossCut.Factories;
using System.Threading;
using System.Threading.Tasks;

public interface IExampleOutputHandler
{
    Task WriteOutputAsync(string message, CancellationToken ct);
    Task RenderMenuAsync(IReadOnlyDictionary<char, ExampleDetails> examples, CancellationToken ct);
    Task ClearScreenAsync(CancellationToken ct);
}
