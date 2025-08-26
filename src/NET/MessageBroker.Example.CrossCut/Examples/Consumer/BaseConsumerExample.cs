using RabbitMQ.Client.Events;
using MessageBroker.Example.CrossCut.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBroker.Example.CrossCut.Examples.Consumer;

public abstract class BaseConsumerExample : IMessageExample<ConsumerExampleStep>
{
    protected readonly IConnectionFactory _connectionFactory;
    protected readonly ILogger _logger;
    protected readonly IExampleInputProvider _inputProvider;
    protected readonly IExampleOutputHandler _outputHandler;
    protected IConnection? _connection;
    protected IChannel? _channel;
    private bool _disposed;

    protected BaseConsumerExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory, IExampleInputProvider exampleInputProvider, IExampleOutputHandler exampleOutputHandler)
    {
        _connectionFactory = connectionFactory;
        _logger = loggerFactory.CreateLogger(this.GetType().Name);
        _inputProvider = exampleInputProvider;
        _outputHandler = exampleOutputHandler;
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


    private bool _isComplete = false;

    public bool IsComplete => _isComplete;


    public async Task<string> ExecuteStepAsync(ConsumerExampleStep step, CancellationToken ct)
    {
        await InitiateConnections(ct);

        switch (step)
        {
            case ConsumerExampleStep.Setup:
                await SetupConsumingQueues(ct);
                return "Setup complete.";
            case ConsumerExampleStep.SendMessages:
                await SendMessageStep(ct);
                return "Message sent.";
            case ConsumerExampleStep.ConsumeMessages:
                await ConsumeMessagesStep(ct);
                return "Consuming messages.";
            default:
                return "Unknown step.";
        }
    }

    public abstract Task SendMessageStep(CancellationToken ct);
    public abstract Task ConsumeMessagesStep(CancellationToken ct);


    private async Task InitiateConnections(CancellationToken ct)
    {
        _connection = await _connectionFactory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
    }

    public abstract Task SetupConsumingQueues(CancellationToken ct);

    // Helper for derived classes to raise the event
    protected async Task OnMessageReceivedAsync(string consumerKey, string message, CancellationToken ct)
    {
        await _outputHandler.WriteOutputAsync($"Message received by {consumerKey}: {message}", ct);
    }
}
