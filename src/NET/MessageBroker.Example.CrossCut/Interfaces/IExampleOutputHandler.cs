namespace MessageBroker.Example.CrossCut.Interfaces;

public interface IExampleOutputHandler
{
    Task WriteOutputAsync(string message);
}
