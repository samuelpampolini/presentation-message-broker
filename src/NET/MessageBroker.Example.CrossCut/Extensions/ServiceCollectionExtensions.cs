using Microsoft.Extensions.DependencyInjection;
using MessageBroker.Example.CrossCut.Factories;
using MessageBroker.Example.CrossCut.Interfaces;
using MessageBroker.Example.CrossCut.Examples.Publisher;
using MessageBroker.Example.CrossCut.Examples.Consumer;

namespace MessageBroker.Example.CrossCut.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageBrokerCrossCut(this IServiceCollection services)
    {
        services.AddSingleton<ExampleFactory>();

        // Register as transient to create a new instance for each example run.
        services.AddTransient<FanoutExample>();
        services.AddTransient<DirectExample>();
        services.AddTransient<TopicExample>();
        services.AddTransient<HeaderExample>();
        services.AddTransient<ExchangeToExchangeExample>();
        services.AddTransient<AlternateExchangeExample>();
        services.AddTransient<SimpleConsumerExample>();
        services.AddTransient<PoisonMessageExample>();

        return services;
    }
}
