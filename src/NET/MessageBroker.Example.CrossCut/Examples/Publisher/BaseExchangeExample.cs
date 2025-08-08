using MessageBroker.Example.CrossCut.Interfaces;
using RabbitMQ.Client;
using System.Text;

namespace MessageBroker.Example.CrossCut.Examples.Publisher;

public abstract class BaseExchangeExample : IMessageExample
{
    protected readonly IConnectionFactory _connectionFactory;
    protected readonly IExampleInputProvider _inputProvider;
    protected readonly IExampleOutputHandler _outputHandler;
    protected IConnection? _connection;
    protected IChannel? _channel;
    private bool _disposed;

    protected abstract string ExchangeName { get; }
    protected abstract string TypeOfExchange { get; }
    protected abstract List<string> QueuesCreated { get; }

    protected BaseExchangeExample(IConnectionFactory connectionFactory,
     IExampleInputProvider inputProvider,
     IExampleOutputHandler outputHandler)
    {
        _connectionFactory = connectionFactory;
        _inputProvider = inputProvider;
        _outputHandler = outputHandler;
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
            _outputHandler.WriteOutputAsync("Disposing resources...", default).Wait();
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

        await _inputProvider.GetInputAsync("Environment is ready, press any Key to send the messages", ct);

        await SendTestMessages(ct);
        await CleanUpTestEnvironment(ct);

        await _outputHandler.WriteOutputAsync("Example completed successfully", ct);
    }

    private async Task InitiateConnections(CancellationToken ct)
    {
        await _outputHandler.WriteOutputAsync("Preparing the environment", ct);

        _connection = await _connectionFactory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
    }

    protected virtual async Task CreateTestEnvironment(CancellationToken ct)
    {
        await _outputHandler.WriteOutputAsync("Creating the Necessary Setup", ct);

        if (_channel is null)
            throw new InvalidOperationException($"Channel not created, please execute {nameof(CreateTestEnvironment)}");

        // Create the exchange with the specified type
        await _channel.ExchangeDeclareAsync(ExchangeName, TypeOfExchange, durable: true, cancellationToken: ct);
    }

    protected virtual async Task<bool> CleanUpTestEnvironment(CancellationToken ct)
    {
        // Leave the environment to check on RabbitMQ interface
        string input = await _inputProvider.GetInputAsync("Do you want to clean up the test Environment? (Y/N)", ct);
        bool cleanUpEnvironment = input.Equals("Y", StringComparison.OrdinalIgnoreCase);
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
        await _outputHandler.WriteOutputAsync($"Sending message: {message}", cancellationToken);

        if (_channel is null)
            throw new InvalidOperationException($"Channel not created, please execute InitiateConnections");

        var body = Encoding.UTF8.GetBytes(message);
        await _channel.BasicPublishAsync(exchange, routingKey, body: body, cancellationToken: cancellationToken);
    }
}
