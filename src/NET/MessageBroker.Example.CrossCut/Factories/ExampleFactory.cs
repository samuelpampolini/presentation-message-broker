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
    private ImmutableSortedDictionary<char, ExampleDetails> _examples;

    public ExampleFactory(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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

    public IMessageExample? CreateExample(char keyChar)
    {
        if (_examples.ContainsKey(keyChar))
        {
            var exampleInformation = _examples[keyChar];
            // get a fresh instance of the example.
            var implementation = _serviceProvider.GetService(exampleInformation.typeOfExample) as IMessageExample;
            return implementation;
        }
        return null;
    }
}
