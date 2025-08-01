namespace MessageBroker.Example.CrossCut;

public interface IExampleInputProvider
{
    Task<string> GetInputAsync(string prompt);
}

public interface IExampleOutputHandler
{
    Task WriteOutputAsync(string message);
}
