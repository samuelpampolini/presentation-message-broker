using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageBroker.Example.CrossCut;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBroker.Examples.Shared.Examples.Publisher;

[Example("Alternate Exchange", key: ConsoleKey.D6)]
public class AlternateExchangeExample : BaseExchangeExample
{
    private const string queue1 = "presentation-alternate-exchange-queue1";
    private const string queue2 = "presentation-alternate-exchange-queue2";
    private const string queue3 = "presentation-alternate-exchange-queue3";

    protected override string ExchangeName => "presentation-alternate-exchange";
    private const string ExchangeNameFanOut = "presentation-alternate-exchange-fanout";
    protected override string TypeOfExchange => ExchangeType.Direct;
    protected override List<string> QueuesCreated => new() { queue1, queue2, queue3 };

    public AlternateExchangeExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory, loggerFactory) { }

    protected override async Task CreateTestEnvironment(CancellationToken ct)
    {
        if (_channel is null)
            throw new InvalidOperationException("Channel not created");

        var argumentsAlternateExchange = new Dictionary<string, object?> {
            {"alternate-exchange", ExchangeNameFanOut}
        };
        // Create the exchange with the specified type
        await _channel.ExchangeDeclareAsync(ExchangeName, TypeOfExchange, durable: true, arguments: argumentsAlternateExchange, cancellationToken: ct);

        // Create the exchange with the specified type
        await _channel.ExchangeDeclareAsync(ExchangeNameFanOut, ExchangeType.Fanout, durable: true, cancellationToken: ct);

        await _channel.QueueDeclareAsync(queue1, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue2, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue3, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);

        _logger.LogInformation("Binding queues with the respective routing keys: abc and dfg");
        await _channel.QueueBindAsync(queue1, ExchangeName, routingKey: "abc", cancellationToken: ct);
        await _channel.QueueBindAsync(queue2, ExchangeName, routingKey: "dfg", cancellationToken: ct);

        // bind to the fan out exchange
        await _channel.QueueBindAsync(queue3, ExchangeNameFanOut, routingKey: "", cancellationToken: ct);
    }

    public override async Task SendTestMessages(CancellationToken ct)
    {
        // Send messages to the exchange
        var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        await SendMessage($"Routed message - {date}", "abc", ExchangeName, ct);
        await SendMessage($"message not routed 1 - {date}", "xyz", ExchangeName, ct);
        await SendMessage($"message not routed 2 - {date}", "aaa", ExchangeName, ct);
    }

    protected override async Task<bool> CleanUpTestEnvironment(CancellationToken ct)
    {
        bool cleanUp = await base.CleanUpTestEnvironment(ct);

        if (cleanUp)
        {
            if (_channel is null)
            {
                throw new InvalidOperationException("Channel not created");
            }

            await _channel.ExchangeDeleteAsync(ExchangeNameFanOut, cancellationToken: ct);
        }

        return cleanUp;
    }
}
