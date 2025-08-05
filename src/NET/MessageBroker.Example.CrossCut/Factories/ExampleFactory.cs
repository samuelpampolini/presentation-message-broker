using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Reflection;
using MessageBroker.Example.CrossCut.Attributes;
using MessageBroker.Example.CrossCut.Interfaces;

namespace MessageBroker.Example.CrossCut.Factories;

record ExampleDetails(string title, Type typeOfExample);

public class ExampleFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly IExampleInputProvider _inputProvider;
    private readonly IExampleOutputHandler _outputHandler;
    private ImmutableSortedDictionary<char, ExampleDetails> _examples;

    public ExampleFactory(
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        IExampleInputProvider inputProvider,
        IExampleOutputHandler outputHandler)
    {
        _serviceProvider = serviceProvider;
        _logger = loggerFactory.CreateLogger("Main");
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

    private IMessageExample? CreateExample(char keyChar)
    {
        if (_examples.ContainsKey(keyChar))
        {
            var exampleInformation = _examples[keyChar];

            _logger.LogInformation("Creating example of type {Type}", exampleInformation.title);

            // get a fresh instance of the example.
            var implementation = _serviceProvider.GetService(exampleInformation.typeOfExample) as IMessageExample;

            return implementation;
        }

        return null;
    }

    public async Task StartTests(CancellationToken ct = default)
    {
        while (true)
        {
            await _outputHandler.WriteOutputAsync("Press the number of the example you want to run.\nPress Escape to end the program or the example after it finishes.", ct);

            foreach (var e in _examples)
            {
                await _outputHandler.WriteOutputAsync($"{e.Key} - {e.Value.title}", ct);
            }

            string input = await _inputProvider.GetInputAsync("Select example (or press Escape): ", ct);
            if (string.IsNullOrEmpty(input))
                continue;

            // Abstracted exit detection for UI-agnostic input providers
            if (input.Equals("Escape", StringComparison.OrdinalIgnoreCase) || (input.Length == 1 && input[0] == 27))
                return;

            char keyChar = input[0];
            IMessageExample? example = CreateExample(keyChar);

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
        }
    }
}
