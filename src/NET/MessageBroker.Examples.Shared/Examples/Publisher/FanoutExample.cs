using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageBroker.Example.CrossCut;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBroker.Examples.Shared.Examples.Publisher;

[Example("Fanout", key: ConsoleKey.D1)]
public class FanoutExample : BaseExchangeExample
{
    private const string queue1 = "presentation-fanout-queue1";
    private const string queue2 = "presentation-fanout-queue2";
    private const string queue3 = "presentation-fanout-queue3";

    protected override string ExchangeName => "presentation-fanout-exchange";
    protected override string TypeOfExchange => ExchangeType.Fanout;
    protected override List<string> QueuesCreated => new() { queue1, queue2, queue3 };

    public FanoutExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory, loggerFactory) { }

    protected override async Task CreateTestEnvironment(CancellationToken ct)
    {
        await base.CreateTestEnvironment(ct);

        if (_channel is null)
            throw new InvalidOperationException("Channel not created");

        await _channel.QueueDeclareAsync(queue1, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue2, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue3, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);

        _logger.LogInformation("Binding queues");
        await _channel.QueueBindAsync(queue1, ExchangeName, routingKey: "", cancellationToken: ct);
        await _channel.QueueBindAsync(queue2, ExchangeName, routingKey: "", cancellationToken: ct);
        await _channel.QueueBindAsync(queue3, ExchangeName, routingKey: "", cancellationToken: ct);
    }

    public override async Task SendTestMessages(CancellationToken ct)
    {
        // Send messages to the exchange
        var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        await SendMessageToDefaultExchange($"Hello Queue 1 - {date}", cancellationToken: ct);
        await SendMessageToDefaultExchange($"Hello Queue 2 - {date}", cancellationToken: ct);
        await SendMessageToDefaultExchange($"Hello Queue 3 - {date}", cancellationToken: ct);
    }
}
