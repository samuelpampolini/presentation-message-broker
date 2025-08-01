namespace MessageBroker.Example.CrossCut.Interfaces;

public interface IMessageExample : IDisposable
{
    Task RunExample(CancellationToken ct);
}
