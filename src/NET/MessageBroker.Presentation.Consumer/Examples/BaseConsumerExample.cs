using MessageBroker.Example.CrossCut;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace MessageBroker.Presentation.Consumer.Examples;
public abstract class BaseConsumerExample : IMessageExample
{
    protected readonly IConnectionFactory _connectionFactory;
    protected readonly ILogger _logger;
    protected IConnection? _connection;
    protected IChannel? _channel;

    protected BaseConsumerExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
    {
        _connectionFactory = connectionFactory;
        _logger = loggerFactory.CreateLogger(this.GetType().Name);
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing resources...");
        if (_channel is not null)
        {
            _channel.Dispose();
            _channel = null;
        }

        if (_connection is not null)
        {
            _connection.Dispose();
            _connection = null;
        }
    }

    public async Task RunExample(CancellationToken ct)
    {
        _logger.LogInformation("Starting the Example");

        await InitiateConnections(ct);

        await SetupConsumingQueues(ct);

        _logger.LogInformation("Press any key to stop this example:");
        Console.ReadKey();

        _logger.LogInformation("Example completed successfully");
    }

    private async Task InitiateConnections(CancellationToken ct)
    {
        _connection = await _connectionFactory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
    }

    public abstract Task SetupConsumingQueues(CancellationToken ct = default);
}
