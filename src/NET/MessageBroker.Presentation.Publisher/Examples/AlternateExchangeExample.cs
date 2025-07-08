using MessageBroker.Example.CrossCut;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBroker.Presentation.Publisher.Examples;

[Example("Alternate Exchange", key: ConsoleKey.D6)]
internal class AlternateExchangeExample : BaseExchangeExample
{
    private const string queue1 = "presentation-alternate-exchange-queue1";
    private const string queue2 = "presentation-alternate-exchange-queue2";
    private const string queue3 = "presentation-alternate-exchange-queue3";

    protected override string ExchangeName => "presentation-alternate-exchange";
    private const string ExchangeNameFanOut = "presentation-alternate-exchange-fanout";
    protected override string TypeOfExchange => ExchangeType.Direct;
    protected override List<string> QueuesCreated => new() { queue1, queue2, queue3 };

    public AlternateExchangeExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory, loggerFactory) { }

    protected override async Task CreateTestEnvironment(CancellationToken ct = default)
    {
        await base.CreateTestEnvironment(ct);

        if (_channel is null)
            throw new InvalidOperationException("Channel not created");

        // Create the exchange with the specified type
        await _channel.ExchangeDeclareAsync(ExchangeNameFanOut, ExchangeType.Fanout, durable: true, cancellationToken: ct);

        await _channel.QueueDeclareAsync(queue1, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue2, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue3, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);

        _logger.LogInformation("Binding queues with the respective routing keys: #.order, new.order.book, and *.order.*");
        await _channel.QueueBindAsync(queue1, ExchangeName, routingKey: "abc", cancellationToken: ct);
        await _channel.QueueBindAsync(queue2, ExchangeName, routingKey: "dfg", cancellationToken: ct);


        // bind to the fanout exchange
        await _channel.QueueBindAsync(queue3, ExchangeName, routingKey: "", cancellationToken: ct);
        await _channel.ExchangeBindAsync(ExchangeName, ExchangeNameFanOut, routingKey: "", cancellationToken: ct);
    }

    public override async Task SendTestMessages(CancellationToken cancellationToken)
    {
        // Send messages to the exchange
        var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        await SendMessage($"New order book - {date}", "new.order.book", ExchangeName, cancellationToken);
        await SendMessage($"New order created - {date}", "new.order", ExchangeNameFanOut, cancellationToken);
    }

    protected override async Task ExtraExampleCleanUp(CancellationToken ct)
    {
        if (_channel is null)
            throw new InvalidOperationException("Channel not created");

        await _channel.ExchangeDeleteAsync(ExchangeNameFanOut, cancellationToken: ct);
    }
}
