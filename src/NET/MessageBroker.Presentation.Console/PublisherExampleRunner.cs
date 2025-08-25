using System;
using System.Threading;
using System.Threading.Tasks;
using MessageBroker.Example.CrossCut.Interfaces;

namespace MessageBroker.Presentation.Console;

public class PublisherExampleRunner
{
    private readonly IExampleInputProvider _inputProvider;
    private readonly IExampleOutputHandler _outputHandler;

    public PublisherExampleRunner(IExampleInputProvider inputProvider, IExampleOutputHandler outputHandler)
    {
        _inputProvider = inputProvider;
        _outputHandler = outputHandler;
    }

    public async Task RunAsync(IMessageExample<ExampleStep> example, CancellationToken cancellationToken)
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
