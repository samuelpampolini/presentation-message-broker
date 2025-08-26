using System;
using System.Threading;
using System.Threading.Tasks;
using MessageBroker.Example.CrossCut.Interfaces;

namespace MessageBroker.Presentation.Console;

public class ExampleRunner
{
    private readonly IExampleInputProvider _inputProvider;
    private readonly IExampleOutputHandler _outputHandler;

    public ExampleRunner(IExampleInputProvider inputProvider, IExampleOutputHandler outputHandler)
    {
        _inputProvider = inputProvider;
        _outputHandler = outputHandler;
    }

    public async Task RunAsync<TStep>(IMessageExample<TStep> example, CancellationToken cancellationToken) where TStep : Enum
    {
        while (!example.IsComplete)
        {
            await ShowStepMenuAsync<TStep>(cancellationToken);
            var stepInput = await _inputProvider.GetInputAsync("Select step to execute (number or X): ", cancellationToken);
            if (string.IsNullOrEmpty(stepInput)) continue;
            if (stepInput.Equals("X", StringComparison.OrdinalIgnoreCase))
            {
                await _outputHandler.WriteOutputAsync("Example terminated by user.", cancellationToken);
                break;
            }
            if (!int.TryParse(stepInput, out int stepNum) || !Enum.IsDefined(typeof(TStep), stepNum))
            {
                await _outputHandler.WriteOutputAsync("Invalid step selection. Please try again.", cancellationToken);
                continue;
            }
            var step = (TStep)Enum.ToObject(typeof(TStep), stepNum);
            string result = await example.ExecuteStepAsync(step, cancellationToken);
            await _outputHandler.WriteOutputAsync($"Step result: {result}", cancellationToken);
            // For publisher, break on CleanUp; for consumer, just loop until complete
            if (typeof(TStep).Name == nameof(ExampleStep) && stepNum == (int)ExampleStep.CleanUp) break;
        }
        if (example.IsComplete)
        {
            await _outputHandler.WriteOutputAsync("Example completed all steps.", cancellationToken);
        }
    }

    private async Task ShowStepMenuAsync<TStep>(CancellationToken cancellationToken) where TStep : Enum
    {
        await _outputHandler.WriteOutputAsync("\nAvailable steps:", cancellationToken);
        foreach (var step in Enum.GetValues(typeof(TStep)))
        {
            await _outputHandler.WriteOutputAsync($"  {(int)step}: {step}", cancellationToken);
        }
        await _outputHandler.WriteOutputAsync("  X: Terminate example", cancellationToken);
    }
}
