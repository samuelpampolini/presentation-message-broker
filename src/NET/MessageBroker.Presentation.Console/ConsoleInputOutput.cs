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
    public Task ClearScreenAsync(CancellationToken ct)
    {
        Console.Clear();
        return Task.CompletedTask;
    }

    public Task WriteOutputAsync(string message, CancellationToken ct)
    {
        Console.WriteLine(message);
        return Task.CompletedTask;
    }

    public Task RenderMenuAsync(IReadOnlyDictionary<char, MessageBroker.Example.CrossCut.Factories.ExampleDetails> examples, CancellationToken ct)
    {
        Console.WriteLine("\nAvailable Examples:");
        Console.WriteLine("+-----+-------------------------------+");
        Console.WriteLine("| Key | Example Name                  |");
        Console.WriteLine("+-----+-------------------------------+");
        foreach (var e in examples)
        {
            Console.WriteLine($"|  {e.Key}  | {e.Value.title,-29} |");
        }
        Console.WriteLine("+-----+-------------------------------+");
        return Task.CompletedTask;
    }
}
