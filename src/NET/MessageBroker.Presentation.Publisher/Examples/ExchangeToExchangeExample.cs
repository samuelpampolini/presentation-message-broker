using MessageBroker.Example.CrossCut;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBroker.Presentation.Publisher.Examples;

[Example("Exchange to Exchange", key: ConsoleKey.D5)]
internal class ExchangeToExchangeExample : BaseExchangeExample
{
    private const string queue1 = "presentation-exchange-to-exchange-queue1";
    private const string queue2 = "presentation-exchange-to-exchange-queue2";
    private const string queue3 = "presentation-exchange-to-exchange-queue3";
    private const string queue4 = "presentation-exchange-to-exchange-queue4";

    protected override string ExchangeName => "presentation-exchange-to-exchange";
    private const string ExchangeNameFanOut = "presentation-exchange-to-exchange-fanout";
    protected override string TypeOfExchange => ExchangeType.Topic;
    protected override List<string> QueuesCreated => new() { queue1, queue2, queue3, queue4 };

    public ExchangeToExchangeExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory, loggerFactory) { }

    protected override async Task CreateTestEnvironment(CancellationToken ct = default)
    {
        // create the basic exchange.
        await base.CreateTestEnvironment(ct);

        if (_channel is null)
            throw new InvalidOperationException($"Channel not created, please execute {nameof(CreateTestEnvironment)}");

        // Create the exchange with the specified type
        await _channel.ExchangeDeclareAsync(ExchangeNameFanOut, ExchangeType.Fanout, durable: true, cancellationToken: ct);

        _logger.LogInformation("Creating queues: {Queues}", string.Join(", ", QueuesCreated));

        await _channel.QueueDeclareAsync(queue1, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue2, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue3, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue4, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);

        _logger.LogInformation("Binding queues with the respective routing keys: #.order, new.order.book, and *.order.*");
        await _channel.QueueBindAsync(queue1, ExchangeName, routingKey: "#.order.#", cancellationToken: ct);
        await _channel.QueueBindAsync(queue2, ExchangeName, routingKey: "new.order.book", cancellationToken: ct);
        await _channel.QueueBindAsync(queue3, ExchangeName, routingKey: "*.order", cancellationToken: ct);

        // bind to the fanout exchange
        await _channel.QueueBindAsync(queue4, ExchangeNameFanOut, routingKey: "", cancellationToken: ct);
        await _channel.ExchangeBindAsync(ExchangeName, ExchangeNameFanOut, routingKey: "", cancellationToken: ct);
    }

    public override async Task SendTestMessages(CancellationToken ct)
    {
        // Send messages to the exchange
        var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        await SendMessage($"New order book - {date}", "new.order.book", ExchangeName, ct);
        await SendMessage($"New order created - {date}", "new.order", ExchangeNameFanOut, ct);
    }

    protected override async Task<bool> CleanUpTestEnvironment(CancellationToken ct)
    {
        bool cleanUp = await base.CleanUpTestEnvironment(ct);

        if (cleanUp)
        {
            if (_channel is null)
                throw new InvalidOperationException("Channel not created");

            await _channel.ExchangeDeleteAsync(ExchangeNameFanOut, cancellationToken: ct);
        }

        return cleanUp;
    }
}
