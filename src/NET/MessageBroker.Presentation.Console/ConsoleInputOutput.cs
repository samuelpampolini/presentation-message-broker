using MessageBroker.Example.CrossCut.Interfaces;

namespace MessageBroker.Presentation.Console;

public class ConsoleInputProvider : IExampleInputProvider
{
    public Task<string> GetInputAsync(string prompt, CancellationToken ct)
    {
        System.Console.Write(prompt);
        var keyInfo = System.Console.ReadKey(intercept: true);
        if (keyInfo.Key == ConsoleKey.Escape)
            return Task.FromResult(((char)27).ToString());
        System.Console.WriteLine(keyInfo.KeyChar);
        return Task.FromResult(keyInfo.KeyChar.ToString());
    }
}

public class ConsoleOutputHandler : IExampleOutputHandler
{
    public Task ClearScreenAsync(CancellationToken ct)
    {
        System.Console.Clear();
        return Task.CompletedTask;
    }

    public Task WriteOutputAsync(string message, CancellationToken ct)
    {
        System.Console.WriteLine(message);
        return Task.CompletedTask;
    }

    public Task RenderMenuAsync(IReadOnlyDictionary<char, MessageBroker.Example.CrossCut.Factories.ExampleDetails> examples, CancellationToken ct)
    {
        System.Console.WriteLine("\nAvailable Examples:");
        System.Console.WriteLine("+-----+-------------------------------+");
        System.Console.WriteLine("| Key | Example Name                  |");
        System.Console.WriteLine("+-----+-------------------------------+");
        foreach (var e in examples)
        {
            System.Console.WriteLine($"|  {e.Key}  | {e.Value.title,-29} |");
        }
        System.Console.WriteLine("+-----+-------------------------------+");
        return Task.CompletedTask;
    }
}
