using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Reflection;

namespace MessageBroker.Example.CrossCut;

record ExampleDetails(string title, Type typeOfExample);

public class ExampleFactory<T>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private ImmutableSortedDictionary<char, ExampleDetails> _examples;

    public ExampleFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;

        _logger = loggerFactory.CreateLogger("Main");
        _examples = ImmutableSortedDictionary<char, ExampleDetails>.Empty;

        LoadExamples();
    }

    private void LoadExamples()
    {
        var loadingDictionary = new Dictionary<char, ExampleDetails>();

        typeof(T).Assembly
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
        Console.Clear();

        while (true)
        {
            _logger.LogInformation("Press the number of the example you want to run.\nPress Escape to end the program or the example after it finishes.");

            _examples.ToList().ForEach((Action<KeyValuePair<char, ExampleDetails>>)(e =>
            {
                _logger.LogInformation("{Key} - {Name}", e.Key, e.Value.title);
            }));

            ConsoleKeyInfo keyInfo = Console.ReadKey();
            Console.WriteLine();

            if (keyInfo.Key == ConsoleKey.Escape)
                return;

            IMessageExample? example = CreateExample(keyInfo.KeyChar);

            if (example is null)
            {
                _logger.LogError("Example not found for key {Key}.", keyInfo);
                return;
            }

            try
            {
                await example.RunExample(ct);
            }
            finally
            {
                example.Dispose();
            }

            _logger.LogInformation("End of test, press any key to continue.");
            Console.ReadKey();
            Console.Clear();
        }
    }
}
