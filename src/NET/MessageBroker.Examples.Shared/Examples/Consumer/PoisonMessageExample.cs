using MessageBroker.Example.CrossCut;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MessageBroker.Examples.Shared.Examples.Consumer;

[Example("Poison Message", key: ConsoleKey.D8)]
public class PoisonMessageExample : BaseConsumerExample
{
    public PoisonMessageExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory, loggerFactory) { }

    public override async Task SetupConsumingQueues(CancellationToken ct = default)
    {
        if (_channel is null)
            throw new InvalidOperationException("Channel is not initialized.");

        var queueName1 = "presentation-poison-message-1";
        var queueName2 = "presentation-poison-message-2";

        var dldQueue1 = "presentation-poison-message-dld-1";
        var dldQueue2 = "presentation-poison-message-dld-2";
        var dldExchange = "presentation-poison-message-dld";

        // Delete existing queues and exchanges if they exist
        await _channel.QueueDeleteAsync(queueName1, cancellationToken: ct);
        await _channel.QueueDeleteAsync(queueName2, cancellationToken: ct);
        await _channel.QueueDeleteAsync(dldQueue1, cancellationToken: ct);
        await _channel.QueueDeleteAsync(dldQueue2, cancellationToken: ct);
        await _channel.ExchangeDeleteAsync(dldExchange, cancellationToken: ct);

        // Declare the dead-letter exchange and queues
        await _channel.ExchangeDeclareAsync(dldExchange, ExchangeType.Direct, durable: true, autoDelete: false, arguments: null, cancellationToken: ct);

        await _channel.QueueDeclareAsync(dldQueue1, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueBindAsync(dldQueue1, dldExchange, routingKey: "poison-message-1", cancellationToken: ct);

        await _channel.QueueDeclareAsync(dldQueue2, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueBindAsync(dldQueue2, dldExchange, routingKey: "poison-message-2", cancellationToken: ct);

        // Header configuration of the normal queues.
        var queue1Arguments = new Dictionary<string, object?> {
            {"x-queue-type", "quorum"},
            {"x-dead-letter-exchange", dldExchange},
            {"x-dead-letter-routing-key", "poison-message-1"},
            {"x-delivery-limit", 3},
            //{"x-message-ttl", 20000}
        };
        await _channel.QueueDeclareAsync(queueName1, durable: true, exclusive: false, autoDelete: false, arguments: queue1Arguments, cancellationToken: ct);

        var queue2Arguments = new Dictionary<string, object?> {
            {"x-queue-type", "quorum"},
            {"x-dead-letter-exchange", dldExchange},
            {"x-dead-letter-routing-key", "poison-message-2"},
            {"x-delivery-limit", 4},
            //{"x-message-ttl", 20000}
        };
        await _channel.QueueDeclareAsync(queueName2, durable: true, exclusive: false, autoDelete: false, arguments: queue2Arguments, cancellationToken: ct);
    }
}
