using MessageBroker.Example.CrossCut;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MessageBroker.Presentation.Consumer.Examples;

[Example("Poison Message", key: ConsoleKey.D2)]
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

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += Consumer_ReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: queueName1,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct
        );

        await _channel.BasicConsumeAsync(
            queue: queueName2,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct
        );

        _channel.CallbackExceptionAsync += CallbackExceptionAsync;
    }

    private Task CallbackExceptionAsync(object sender, CallbackExceptionEventArgs args)
    {
        _logger.LogInformation("An error occurred in the consumer. [{Message}]", args.Exception.Message);
        return Task.CompletedTask;
    }

    private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        try
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Message: {Message}, Delivery Tag: {DeliveryTag}, Exchange: {Exchange}, and Routing Key: {RoutingKey}", message, eventArgs.DeliveryTag, eventArgs.Exchange, eventArgs.RoutingKey);

            // Simulate processing time
            await Task.Delay(500);

            if (message.Contains("fail"))
            {
                throw new InvalidOperationException("Simulated processing unexpected failure for poison message.");
            }

            if (_channel is null)
                throw new InvalidOperationException("Channel is not initialized.");

            await _channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: true);
        }
        catch
        {
            if (_channel is not null)
                await _channel.BasicRejectAsync(deliveryTag: eventArgs.DeliveryTag, requeue: true);

            throw;
        }
    }
}
