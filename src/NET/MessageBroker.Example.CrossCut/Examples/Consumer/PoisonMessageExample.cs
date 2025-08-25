using System.Threading.Tasks;
using MessageBroker.Example.CrossCut.Attributes;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBroker.Example.CrossCut.Examples.Consumer;

[Example("Poison Message", key: ConsoleKey.D8)]
public class PoisonMessageExample : BaseConsumerExample
{
    public PoisonMessageExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory, loggerFactory)
    {
    }


    public override async Task SetupConsumingQueues(CancellationToken ct)
    {
        // Step 1: Setup (configure exchange and queues)
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);
        using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        var queueName1 = "presentation-poison-message-1";
        var queueName2 = "presentation-poison-message-2";
        var dldQueue1 = "presentation-poison-message-dld-1";
        var dldQueue2 = "presentation-poison-message-dld-2";
        var dldExchange = "presentation-poison-message-dld";

        await channel.QueueDeleteAsync(queueName1, false, false, cancellationToken: ct);
        await channel.QueueDeleteAsync(queueName2, false, false, cancellationToken: ct);
        await channel.QueueDeleteAsync(dldQueue1, false, false, cancellationToken: ct);
        await channel.QueueDeleteAsync(dldQueue2, false, false, cancellationToken: ct);
        await channel.ExchangeDeleteAsync(dldExchange, false, cancellationToken: ct);

        await channel.ExchangeDeclareAsync(dldExchange, ExchangeType.Direct, durable: true, autoDelete: false, arguments: null, cancellationToken: ct);

        await channel.QueueDeclareAsync(dldQueue1, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await channel.QueueBindAsync(dldQueue1, dldExchange, routingKey: "poison-message-1", cancellationToken: ct);

        await channel.QueueDeclareAsync(dldQueue2, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await channel.QueueBindAsync(dldQueue2, dldExchange, routingKey: "poison-message-2", cancellationToken: ct);

        var queue1Arguments = new Dictionary<string, object?> {
            {"x-queue-type", "quorum"},
            {"x-dead-letter-exchange", dldExchange},
            {"x-dead-letter-routing-key", "poison-message-1"},
            {"x-delivery-limit", 3},
        };
        await channel.QueueDeclareAsync(queueName1, durable: true, exclusive: false, autoDelete: false, arguments: queue1Arguments, cancellationToken: ct);

        var queue2Arguments = new Dictionary<string, object?> {
            {"x-queue-type", "quorum"},
            {"x-dead-letter-exchange", dldExchange},
            {"x-dead-letter-routing-key", "poison-message-2"},
            {"x-delivery-limit", 4},
        };
        await channel.QueueDeclareAsync(queueName2, durable: true, exclusive: false, autoDelete: false, arguments: queue2Arguments, cancellationToken: ct);
    }

    public override async Task SendMessageStep(CancellationToken ct)
    {
        // Step 2: Send Message
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);
        using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
        var body = System.Text.Encoding.UTF8.GetBytes($"Test Poison Message: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        await channel.BasicPublishAsync(exchange: "", routingKey: "presentation-poison-message-1", body: body, cancellationToken: ct);
    }

    public override async Task ConsumeMessagesStep(CancellationToken ct)
    {
        // Step 3: Consume Messages
        using var connection = await _connectionFactory.CreateConnectionAsync(ct);
        using var channel = await connection.CreateChannelAsync(cancellationToken: ct);
        var consumer = new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = System.Text.Encoding.UTF8.GetString(body);
            OnMessageReceived("poison-message-1", message);
            await channel.BasicAckAsync(ea.DeliveryTag, false);
        };
        await channel.BasicConsumeAsync(queue: "presentation-poison-message-1", autoAck: false, consumer: consumer);
        // Optionally, add a delay or cancellation logic here
    }
}
