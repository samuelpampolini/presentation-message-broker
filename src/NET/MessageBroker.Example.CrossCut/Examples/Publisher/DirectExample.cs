using MessageBroker.Example.CrossCut.Attributes;
using MessageBroker.Example.CrossCut.Interfaces;
using RabbitMQ.Client;

namespace MessageBroker.Example.CrossCut.Examples.Publisher;

[Example("Direct", key: ConsoleKey.D2)]
public class DirectExample : BaseExchangeExample
{
    private const string queue1 = "presentation-direct-queue1";
    private const string queue2 = "presentation-direct-queue2";
    private const string queue3 = "presentation-direct-queue3";

    private const string routingKeyOrderUpdate = "order.update";
    private const string routingKeyNewOrder = "new.order";

    protected override string ExchangeName => "presentation-direct-exchange";
    protected override string TypeOfExchange => ExchangeType.Direct;
    protected override List<string> QueuesCreated => new() { queue1, queue2, queue3 };

    public DirectExample(IConnectionFactory connectionFactory,
        IExampleInputProvider inputProvider,
        IExampleOutputHandler outputHandler) : base(connectionFactory, inputProvider, outputHandler) { }

    protected override async Task CreateTestEnvironment(CancellationToken ct)
    {
        await base.CreateTestEnvironment(ct);

        if (_channel is null)
            throw new InvalidOperationException("Channel not created");

        await _channel.QueueDeclareAsync(queue1, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue2, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue3, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);

        await _outputHandler.WriteOutputAsync($"Binding queues with the respective routing keys: {routingKeyOrderUpdate}, and {routingKeyNewOrder}", ct);
        await _channel.QueueBindAsync(queue1, ExchangeName, routingKey: routingKeyOrderUpdate, cancellationToken: ct);
        await _channel.QueueBindAsync(queue2, ExchangeName, routingKey: routingKeyNewOrder, cancellationToken: ct);
        await _channel.QueueBindAsync(queue3, ExchangeName, routingKey: routingKeyNewOrder, cancellationToken: ct);
    }

    public override async Task SendTestMessages(CancellationToken ct)
    {
        // Send messages to the exchange
        var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        await SendMessageToDefaultExchange($"Test of an updated order - {date}", routingKeyOrderUpdate, ct);
        await SendMessageToDefaultExchange($"New order created 1 - {date}", routingKeyNewOrder, ct);
        await SendMessageToDefaultExchange($"New order created 2 - {date}", routingKeyNewOrder, ct);
    }
}
