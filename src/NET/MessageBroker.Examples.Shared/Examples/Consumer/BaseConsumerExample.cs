using MessageBroker.Example.CrossCut;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBroker.Examples.Shared.Examples.Consumer;

public abstract class BaseConsumerExample : IMessageExample
{
    protected readonly IConnectionFactory _connectionFactory;
    protected readonly ILogger _logger;
    protected IConnection? _connection;
    protected IChannel? _channel;
    private bool _disposed;

    protected BaseConsumerExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
    {
        _connectionFactory = connectionFactory;
        _logger = loggerFactory.CreateLogger(this.GetType().Name);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
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

        _disposed = true;
    }

    public async Task RunExample(CancellationToken ct)
    {
        _logger.LogInformation("Starting the Example");

        await InitiateConnections(ct);
        await SetupConsumingQueues(ct);

        _logger.LogInformation("Press any key to stop this example:");
        Console.ReadKey();
    }

    private async Task InitiateConnections(CancellationToken ct)
    {
        _connection = await _connectionFactory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
    }

    public abstract Task SetupConsumingQueues(CancellationToken ct = default);
}
