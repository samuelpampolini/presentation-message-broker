using MessageBroker.Example.CrossCut.Attributes;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBroker.Example.CrossCut.Examples.Publisher;

[Example("Topic", key: ConsoleKey.D3)]
public class TopicExample : BaseExchangeExample
{
    private const string queue1 = "presentation-topic-queue1";
    private const string queue2 = "presentation-topic-queue2";
    private const string queue3 = "presentation-topic-queue3";

    protected override string ExchangeName => "presentation-topic-exchange";
    protected override string TypeOfExchange => ExchangeType.Topic;
    protected override List<string> QueuesCreated => new() { queue1, queue2, queue3 };

    public TopicExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory, loggerFactory) { }

    protected override async Task CreateTestEnvironment(CancellationToken ct)
    {
        await base.CreateTestEnvironment(ct);

        if (_channel is null)
            throw new InvalidOperationException("Channel not created");

        await _channel.QueueDeclareAsync(queue1, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue2, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue3, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);

        _logger.LogInformation("Binding queues with the respective routing keys: #.order.#, new.order.book, and *.order.*");
        await _channel.QueueBindAsync(queue1, ExchangeName, routingKey: "#.order.#", cancellationToken: ct);
        await _channel.QueueBindAsync(queue2, ExchangeName, routingKey: "new.order.book", cancellationToken: ct);
        await _channel.QueueBindAsync(queue3, ExchangeName, routingKey: "*.order", cancellationToken: ct);
    }

    public override async Task SendTestMessages(CancellationToken ct)
    {
        // Send messages to the exchange
        var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        await SendMessageToDefaultExchange($"New order book - {date}", "new.order.book", ct);
        await SendMessageToDefaultExchange($"New order created - {date}", "new.order", ct);
        await SendMessageToDefaultExchange($"New order created - {date}", "order", ct);
    }
}
