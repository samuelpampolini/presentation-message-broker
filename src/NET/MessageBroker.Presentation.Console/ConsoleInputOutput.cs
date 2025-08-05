using MessageBroker.Example.CrossCut.Interfaces;

namespace MessageBroker.Presentation.ConsoleIO;

public class ConsoleInputProvider : IExampleInputProvider
{
    public Task<string> GetInputAsync(string prompt, CancellationToken ct)
    {
        Console.Write(prompt);
        var keyInfo = Console.ReadKey(intercept: true);
        if (keyInfo.Key == ConsoleKey.Escape)
            return Task.FromResult(((char)27).ToString());
        Console.WriteLine(keyInfo.KeyChar);
        return Task.FromResult(keyInfo.KeyChar.ToString());
    }
}

public class ConsoleOutputHandler : IExampleOutputHandler
{
    public Task WriteOutputAsync(string message, CancellationToken ct)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }
}
