using MessageBroker.Example.CrossCut.Factories;
using MessageBroker.Example.CrossCut.Interfaces;

namespace MessageBroker.Presentation.Console;

public class ExampleTestRunner
{
    private readonly ExampleFactory _exampleFactory;
    private readonly IExampleInputProvider _inputProvider;
    private readonly IExampleOutputHandler _outputHandler;

    public ExampleTestRunner(ExampleFactory exampleFactory, IExampleInputProvider inputProvider, IExampleOutputHandler outputHandler)
    {
        _exampleFactory = exampleFactory;
        _inputProvider = inputProvider;
        _outputHandler = outputHandler;
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
            var example = _exampleFactory.CreateExample(keyChar);
            if (example is null)
            {
                await _outputHandler.WriteOutputAsync($"Example not found for key {keyChar}. Please try again.", cancellationToken);
                continue;
            }

            try
            {
                await RunExampleStepsAsync(example, cancellationToken);
            }
            finally
            {
                if (example is IDisposable disposable)
                    disposable.Dispose();
            }

        }
    }

    private async Task RunExampleStepsAsync(IMessageExample example, CancellationToken cancellationToken)
    {
        while (!example.IsComplete)
        {
            await ShowStepMenuAsync(cancellationToken);
            var stepInput = await _inputProvider.GetInputAsync("Select step to execute (number or X): ", cancellationToken);
            if (string.IsNullOrEmpty(stepInput)) continue;
            if (stepInput.Equals("X", StringComparison.OrdinalIgnoreCase))
            {
                await _outputHandler.WriteOutputAsync("Example terminated by user.", cancellationToken);
                break;
            }
            if (!int.TryParse(stepInput, out int stepNum) || !Enum.IsDefined(typeof(ExampleStep), stepNum))
            {
                await _outputHandler.WriteOutputAsync("Invalid step selection. Please try again.", cancellationToken);
                continue;
            }
            var step = (ExampleStep)stepNum;
            string result = await example.ExecuteStepAsync(step, cancellationToken);
            await _outputHandler.WriteOutputAsync($"Step result: {result}", cancellationToken);
            if (step == ExampleStep.CleanUp) break;
        }
        if (example.IsComplete)
        {
            await _outputHandler.WriteOutputAsync("Example completed all steps.", cancellationToken);
        }
    }

    private async Task ShowStepMenuAsync(CancellationToken cancellationToken)
    {
        await _outputHandler.WriteOutputAsync("\nAvailable steps:", cancellationToken);
        foreach (var step in Enum.GetValues(typeof(ExampleStep)))
        {
            await _outputHandler.WriteOutputAsync($"  {(int)step}: {step}", cancellationToken);
        }
        await _outputHandler.WriteOutputAsync("  X: Terminate example", cancellationToken);
    }
}
