using MessageBroker.Example.CrossCut;
using MessageBroker.Presentation.Consumer.Examples;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

// Build a config object, using env vars and JSON providers.
IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.Configure<IConfiguration>(config);
services.AddSingleton<ExampleFactory<Program>>();

// Register as transient to create a new instance for each example run.
services.AddTransient<SimpleConsumerExample>();
services.AddTransient<PoisonMessageExample>();

// Add Logs
services.AddLogging(builder =>
{
    builder.SetMinimumLevel(LogLevel.Trace);
    builder.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.TimestampFormat = "{HH:mm:ss} ";
        options.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
    });
});

// Get values from the config given their key and their target type.
RabbitMQSettings settings = config.GetRequiredSection("MessageBoker:RabbitMQ").Get<RabbitMQSettings>()!;
services.AddSingleton<IConnectionFactory>(serviceProvider =>
{
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
var exampleFactory = serviceProvider.GetRequiredService<ExampleFactory<Program>>();

await exampleFactory.StartTests();