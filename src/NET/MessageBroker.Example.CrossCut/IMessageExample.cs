namespace MessageBroker.Example.CrossCut;

public interface IMessageExample : IDisposable
{
    Task RunExample(CancellationToken ct);
}
