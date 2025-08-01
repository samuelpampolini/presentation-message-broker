namespace MessageBroker.Example.CrossCut.Interfaces;

public interface IExampleInputProvider
{
    Task<string> GetInputAsync(string prompt);
}
