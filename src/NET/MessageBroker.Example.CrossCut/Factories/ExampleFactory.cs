using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Reflection;
using MessageBroker.Example.CrossCut.Attributes;
using MessageBroker.Example.CrossCut.Interfaces;

namespace MessageBroker.Example.CrossCut.Factories;

public record ExampleDetails(string title, Type typeOfExample);

public class ExampleFactory
{
    public IReadOnlyDictionary<char, ExampleDetails> GetExamples()
    {
        return _examples;
    }
    private readonly IServiceProvider _serviceProvider;
    private readonly IExampleInputProvider _inputProvider;
    private readonly IExampleOutputHandler _outputHandler;
    private ImmutableSortedDictionary<char, ExampleDetails> _examples;

    public ExampleFactory(
        IServiceProvider serviceProvider,
        IExampleInputProvider inputProvider,
        IExampleOutputHandler outputHandler)
    {
        _serviceProvider = serviceProvider;
        _inputProvider = inputProvider;
        _outputHandler = outputHandler;
        _examples = ImmutableSortedDictionary<char, ExampleDetails>.Empty;
        LoadExamples();
    }

    private void LoadExamples()
    {
        var loadingDictionary = new Dictionary<char, ExampleDetails>();

        typeof(ExampleFactory).Assembly
           .GetTypes()
           .Where(typeOfExample => typeOfExample.GetCustomAttributes<ExampleAttribute>().Any())
           .ToList()
           .ForEach(typeOfExample =>
           {
               var attribute = typeOfExample.GetCustomAttribute<ExampleAttribute>();

               if (attribute != null)
                   loadingDictionary.Add((char)attribute.Key, new ExampleDetails(attribute.Name, typeOfExample));
           });

        _examples = loadingDictionary.ToImmutableSortedDictionary();
    }

    private async Task<IMessageExample?> CreateExample(char keyChar, CancellationToken ct)
    {
        if (_examples.ContainsKey(keyChar))
        {
            var exampleInformation = _examples[keyChar];

            await _outputHandler.WriteOutputAsync($"Creating example of type {exampleInformation.title}", ct);

            // get a fresh instance of the example.
            var implementation = _serviceProvider.GetService(exampleInformation.typeOfExample) as IMessageExample;

            return implementation;
        }

        return null;
    }


    // The output handler is now responsible for rendering the menu

    public async Task StartTests(CancellationToken ct = default)
    {
        while (true)
        {
            // Delegate menu rendering to the output handler
            await _outputHandler.RenderMenuAsync(_examples, ct);

            string input = await _inputProvider.GetInputAsync("Select example (or press Escape): ", ct);
            if (string.IsNullOrEmpty(input))
                continue;

            // Abstracted exit detection for UI-agnostic input providers
            if (input.Equals("Escape", StringComparison.OrdinalIgnoreCase) || (input.Length == 1 && input[0] == 27))
                return;

            char keyChar = input[0];
            IMessageExample? example = await CreateExample(keyChar, ct);

            if (example is null)
            {
                await _outputHandler.WriteOutputAsync($"Example not found for key {keyChar}. Please try again.", ct);
                continue;
            }

            try
            {
                await example.RunExample(ct);
            }
            finally
            {
                if (example is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            // Clear console after example completes
            await _outputHandler.WriteOutputAsync("\nPress any key to continue...", ct);
            await _inputProvider.GetInputAsync("", ct);
            await _outputHandler.ClearScreenAsync(ct);
        }
    }
}
