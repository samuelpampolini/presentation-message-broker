﻿using MessageBroker.Example.CrossCut;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace MessageBroker.Presentation.Consumer.Examples;
[Example("Simple Consumer", key: ConsoleKey.D1)]
public class SimpleConsumerExample : BaseConsumerExample
{
    public SimpleConsumerExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory, loggerFactory) { }

    public override async Task SetupConsumingQueues(CancellationToken ct = default)
    {
        if(_channel is null)
            throw new InvalidOperationException("Channel is not initialized.");

        var queueName = "presentation-simple-consumer";
        await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += Consumer_ReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct
        );
    }

    private async Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        var body = eventArgs.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        _logger.LogInformation($"Message: {message}, Delivery Tag: {eventArgs.DeliveryTag}, Exchange: {eventArgs.Exchange}, and Routing Key: {eventArgs.RoutingKey}");
        
        // Simulate processing time
        await Task.Delay(500);

        if (_channel is null)
            throw new InvalidOperationException("Channel is not initialized.");

        // Confirm the message processing
        await _channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false);
    }
}
