using MessageBroker.Example.CrossCut.Factories;
using MessageBroker.Example.CrossCut.Interfaces;

namespace MessageBroker.Presentation.Console;

public class ExampleTestRunner
{
    private readonly ExampleFactory _exampleFactory;
    private readonly IExampleInputProvider _inputProvider;
    private readonly IExampleOutputHandler _outputHandler;
    private readonly ExampleRunner _exampleRunner;

    public ExampleTestRunner(ExampleFactory exampleFactory, IExampleInputProvider inputProvider, IExampleOutputHandler outputHandler)
    {
        _exampleFactory = exampleFactory;
        _inputProvider = inputProvider;
        _outputHandler = outputHandler;
        _exampleRunner = new ExampleRunner(inputProvider, outputHandler);
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            await _outputHandler.RenderMenuAsync(_exampleFactory.GetExamples(), cancellationToken);
            var input = await _inputProvider.GetInputAsync("Select example (or press Escape): ", cancellationToken);
            if (string.IsNullOrEmpty(input)) continue;
            if (input.Equals("Escape", StringComparison.OrdinalIgnoreCase) || (input.Length == 1 && input[0] == 27)) break;

            char keyChar = input[0];
            var details = _exampleFactory.GetExamples().TryGetValue(keyChar, out var d) ? d : null;
            if (details == null)
            {
                await _outputHandler.WriteOutputAsync($"Example not found for key {keyChar}. Please try again.", cancellationToken);
                continue;
            }

            var typeOfExample = details.typeOfExample;
            if (typeof(IMessageExample<ExampleStep>).IsAssignableFrom(typeOfExample))
            {
                var example = _exampleFactory.CreateExample<ExampleStep>(keyChar);
                if (example is null)
                {
                    await _outputHandler.WriteOutputAsync($"Failed to create publisher example for key {keyChar}.", cancellationToken);
                    continue;
                }
                try { await _exampleRunner.RunAsync(example, cancellationToken); }
                finally { example.Dispose(); }
            }
            else if (typeof(IMessageExample<ConsumerExampleStep>).IsAssignableFrom(typeOfExample))
            {
                var example = _exampleFactory.CreateExample<ConsumerExampleStep>(keyChar);
                if (example is null)
                {
                    await _outputHandler.WriteOutputAsync($"Failed to create consumer example for key {keyChar}.", cancellationToken);
                    continue;
                }
                try { await _exampleRunner.RunAsync(example, cancellationToken); }
                finally { example.Dispose(); }
            }
            else
            {
                await _outputHandler.WriteOutputAsync($"Unknown example type for key {keyChar}.", cancellationToken);
            }
        }
    }
}
