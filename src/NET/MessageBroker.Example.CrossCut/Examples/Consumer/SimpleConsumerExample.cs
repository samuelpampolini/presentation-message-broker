using MessageBroker.Example.CrossCut.Attributes;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MessageBroker.Example.CrossCut.Examples.Consumer;

[Example("Simple Consumer", key: ConsoleKey.D7)]

/// <summary>
/// Consumer Example Steps:
///   1. SetupConsumingQueues: Declare and prepare the queue for consumption.
///   2. SendMessageStep: (for test/demo) Send a message to the queue.
///   3. ConsumeMessagesStep: Start consuming messages and return them to the caller (no explicit cleanup step).
/// </summary>
public class SimpleConsumerExample : BaseConsumerExample
{
    public SimpleConsumerExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory, loggerFactory) { }

    /// <summary>
    /// Step 1: Setup the queue for consuming messages.
    /// </summary>
    public override async Task SetupConsumingQueues(CancellationToken ct)
    {
        if (_channel is null)
            throw new InvalidOperationException("Channel is not initialized.");

        var queueName = "presentation-simple-consumer";
        await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
    }

    /// <summary>
    /// Step 2: Send a test message to the queue (for demonstration or testing).
    /// </summary>
    public override async Task SendMessageStep(CancellationToken ct)
    {
        if (_channel is null)
            throw new InvalidOperationException("Channel is not initialized.");
        var body = Encoding.UTF8.GetBytes($"Simple Test Message: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        await _channel.BasicPublishAsync(exchange: "", routingKey: "presentation-simple-consumer", body: body, cancellationToken: ct);
    }

    /// <summary>
    /// Step 3: Consume messages from the queue and return them to the calling service.
    /// This step will run and deliver messages as they are received. There is no explicit cleanup step for consumers.
    /// </summary>
    public override async Task ConsumeMessagesStep(CancellationToken ct)
    {
        if (_channel is null)
            throw new InvalidOperationException("Channel is not initialized.");
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            // Return or forward the message to the calling service (e.g., via callback/event/SignalR/gRPC stream)
            OnMessageReceived("simple-consumer", message);
            await Task.Delay(500);
            await _channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false);
        };
        await _channel.BasicConsumeAsync(
            queue: "presentation-simple-consumer",
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct
        );
    }

    /// <summary>
    /// Utility: Send a custom message to the queue.
    /// </summary>
    public async Task SendCustomMessage(string message, CancellationToken ct)
    {
        if (_channel is null)
            throw new InvalidOperationException("Channel is not initialized.");
        var body = Encoding.UTF8.GetBytes(message);
        await _channel.BasicPublishAsync(exchange: "", routingKey: "presentation-simple-consumer", body: body, cancellationToken: ct);
    }
}
