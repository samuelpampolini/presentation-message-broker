using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageBroker.Example.CrossCut;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MessageBroker.Examples.Shared.Examples.Publisher;

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
                throw new InvalidOperationException($"Channel not created, please execute {nameof(InitiateConnections)}");

            await _channel.ExchangeDeleteAsync(ExchangeName, cancellationToken: ct);

            QueuesCreated.ForEach(async queue =>
            {
                _logger.LogInformation("Deleting queue: {Queue}", queue);
                await _channel.QueueDeleteAsync(queue, cancellationToken: ct);
            });
        }

        return cleanUpEnvironment;
    }

    protected Task SendMessageToDefaultExchange<T>(T message, string routingKey = "", CancellationToken cancellationToken = default)
    {
        return SendMessage(message, routingKey, ExchangeName, cancellationToken);
    }

    protected async Task SendMessage<T>(T message, string routingKey = "", string? exchange = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Sending message");

        if (_channel is null)
            throw new InvalidOperationException($"Channel not created, please execute {nameof(InitiateConnections)}");

        string textMessage;

        if (message is string stringMessage)
        {
            textMessage = stringMessage;
        }
        else
        {
            textMessage = System.Text.Json.JsonSerializer.Serialize(message);
        }

        var body = System.Text.Encoding.UTF8.GetBytes(textMessage);

        // Use the provided exchange or the default ExchangeName
        string exchangeToPublish = exchange ?? ExchangeName;

        _logger.LogInformation("Message: [{Message}] with routing key [{RoutingKey}] to [{Exchange}]", textMessage, routingKey, exchangeToPublish);
        await _channel.BasicPublishAsync(exchangeToPublish, routingKey, mandatory: true, body: body, cancellationToken: cancellationToken);
    }

    public abstract Task SendTestMessages(CancellationToken ct);
}
