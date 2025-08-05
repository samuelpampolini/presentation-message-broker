using MessageBroker.Example.CrossCut.Interfaces;
using MessageBroker.Example.CrossCut.Factories;
using MessageBroker.Example.CrossCut.Settings;
using MessageBroker.Example.CrossCut.Extensions;
using MessageBroker.Presentation.ConsoleIO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using MessageBroker.Example.CrossCut.Attributes;

// Build a config object, using env vars and JSON providers.
IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.Configure<IConfiguration>(config);
services.AddMessageBrokerCrossCut();
services.AddSingleton<IExampleInputProvider, ConsoleInputProvider>();
services.AddSingleton<IExampleOutputHandler, ConsoleOutputHandler>();

// Add Logs
services.AddLogging(builder =>
{
    builder.SetMinimumLevel(LogLevel.Trace);
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = false;
        options.SingleLine = true;
        options.TimestampFormat = "{HH:mm:ss} ";
        options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
    });
});


services.AddSingleton<IConnectionFactory>(serviceProvider =>
{
    // Get values from the config given their key and their target type.
    RabbitMQSettings settings = config.GetRequiredSection("MessageBroker:RabbitMQ").Get<RabbitMQSettings>()!;
    var connectionFactory = new ConnectionFactory
    {
        HostName = settings.Host, // Use values from configuration
        Port = settings.Port,
        UserName = settings.UserName,
        Password = settings.Password
    };
    return connectionFactory;
});

var serviceProvider = services.BuildServiceProvider();
var exampleFactory = serviceProvider.GetRequiredService<ExampleFactory>();
await exampleFactory.StartTests();
