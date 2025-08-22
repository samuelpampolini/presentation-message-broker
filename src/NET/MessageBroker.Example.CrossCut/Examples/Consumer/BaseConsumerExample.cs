using MessageBroker.Example.CrossCut.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBroker.Example.CrossCut.Examples.Consumer;

public abstract class BaseConsumerExample : IMessageExample
{
    protected readonly IConnectionFactory _connectionFactory;
    protected readonly ILogger _logger;
    protected readonly IExampleInputProvider _inputProvider;
    protected readonly IExampleOutputHandler _outputHandler;
    protected IConnection? _connection;
    protected IChannel? _channel;
    private bool _disposed;

    protected BaseConsumerExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
    {
        _connectionFactory = connectionFactory;
        _logger = loggerFactory.CreateLogger(this.GetType().Name);
        // These should be injected in derived classes
        _inputProvider = null!;
        _outputHandler = null!;
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

    public async Task<string> ExecuteStepAsync(ExampleStep step, CancellationToken ct)
    {
        switch (step)
        {
            case ExampleStep.Setup:
                await InitiateConnections(ct);
                return "Connections initiated.";
            case ExampleStep.SendMessages:
                await SetupConsumingQueues(ct);
                return "Consuming queues set up.";
            case ExampleStep.CleanUp:
                Dispose();
                _isComplete = true;
                return "Cleaned up and disposed.";
            default:
                return "Unknown step.";
        }
    }

    [Obsolete("Use ExecuteStepAsync instead.")]
    public async Task RunExample(CancellationToken ct)
    {
        _logger.LogInformation("Starting the Example");
        await InitiateConnections(ct);
        await SetupConsumingQueues(ct);
        _logger.LogInformation("Press any key to stop this example:");
        if (_outputHandler != null)
            await _outputHandler.WriteOutputAsync("Press any key to stop this example:", ct);
        if (_inputProvider != null)
            await _inputProvider.GetInputAsync("", ct);
    }

    private async Task InitiateConnections(CancellationToken ct)
    {
        _connection = await _connectionFactory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
    }

    public abstract Task SetupConsumingQueues(CancellationToken ct = default);
}
