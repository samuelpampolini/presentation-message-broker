using MessageBroker.Example.CrossCut.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace MessageBroker.Example.CrossCut.Examples.Publisher;

public abstract class BaseExchangeExample : IMessageExample
{
    protected readonly IConnectionFactory _connectionFactory;
    protected readonly ILogger _logger;
    protected IConnection? _connection;
    protected IChannel? _channel;
    private bool _disposed;

    protected abstract string ExchangeName { get; }
    protected abstract string TypeOfExchange { get; }
    protected abstract List<string> QueuesCreated { get; }

    protected BaseExchangeExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
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
        await InitiateConnections(ct);
        await CreateTestEnvironment(ct);

        _logger.LogInformation("Environment is ready, press any Key to send the messages");
        Console.ReadKey();
        Console.WriteLine();

        await SendTestMessages(ct);
        await CleanUpTestEnvironment(ct);

        _logger.LogInformation("Example completed successfully");
    }

    private async Task InitiateConnections(CancellationToken ct)
    {
        _logger.LogInformation("Preparing the environment");

        _connection = await _connectionFactory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
    }

    protected virtual async Task CreateTestEnvironment(CancellationToken ct)
    {
        _logger.LogInformation("Creating the Necessary Setup");

        if (_channel is null)
            throw new InvalidOperationException($"Channel not created, please execute {nameof(CreateTestEnvironment)}");

        // Create the exchange with the specified type
        await _channel.ExchangeDeclareAsync(ExchangeName, TypeOfExchange, durable: true, cancellationToken: ct);
    }

    protected virtual async Task<bool> CleanUpTestEnvironment(CancellationToken ct)
    {
        // Leave the environment to check on RabbitMQ interface
        _logger.LogInformation("Do you want to clean up the test Environment? (Y/N)");
        bool cleanUpEnvironment = Console.ReadKey().Key == ConsoleKey.Y;
        // Clean up the environment
        if (cleanUpEnvironment)
        {
            if (_channel is null)
                throw new InvalidOperationException($"Channel not created, please execute {nameof(CleanUpTestEnvironment)}");

            foreach (var queue in QueuesCreated)
            {
                await _channel.QueueDeleteAsync(queue, cancellationToken: ct);
            }
            await _channel.ExchangeDeleteAsync(ExchangeName, cancellationToken: ct);
        }
        return cleanUpEnvironment;
    }

    public abstract Task SendTestMessages(CancellationToken ct);

    protected async Task SendMessageToDefaultExchange(string message, string routingKey = "", CancellationToken cancellationToken = default)
    {
        await SendMessage(message, routingKey, ExchangeName, cancellationToken);
    }

    protected async Task SendMessage(string message, string routingKey, string exchange, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Sending message: {message}");

        if (_channel is null)
            throw new InvalidOperationException($"Channel not created, please execute InitiateConnections");

        var body = Encoding.UTF8.GetBytes(message);
        await _channel.BasicPublishAsync(exchange, routingKey, body: body, cancellationToken: cancellationToken);
    }
}
