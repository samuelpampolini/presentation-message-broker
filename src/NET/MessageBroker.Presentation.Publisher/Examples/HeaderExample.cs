using MessageBroker.Example.CrossCut;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MessageBroker.Presentation.Publisher.Examples;

[Example("Header", key: ConsoleKey.D4)]
internal class HeaderExample : BaseExchangeExample
{
    private const string queue1 = "presentation-header-queue1";
    private const string queue2 = "presentation-header-queue2";

    protected override string ExchangeName => "presentation-header-exchange";
    protected override string TypeOfExchange => ExchangeType.Headers;
    protected override List<string> QueuesCreated => new() { queue1, queue2 };

    public HeaderExample(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory) : base(connectionFactory, loggerFactory) { }

    protected override async Task CreateTestEnvironment(CancellationToken ct = default)
    {
        await base.CreateTestEnvironment(ct);

        if (_channel is null)
            throw new InvalidOperationException("Channel not created");

        await _channel.QueueDeclareAsync(queue1, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueDeclareAsync(queue2, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);

        _logger.LogInformation("Binding queues with the respective routing keys: #.order, new.order.book, and *.order.*");

        var headerQueue1 = new Dictionary<string, object?> {
            {"x-match", "all"},
            {"job", "convert"},
            {"format", "jpeg"}
        };
        await _channel.QueueBindAsync(queue1, ExchangeName, routingKey: string.Empty, arguments: headerQueue1, cancellationToken: ct);

        var headerQueue2 = new Dictionary<string, object?> {
            {"x-match", "any"},
            {"job", "convert"},
            {"format", "jpeg"}
        };
        await _channel.QueueBindAsync(queue2, ExchangeName, routingKey: string.Empty, arguments: headerQueue2, cancellationToken: ct);
    }

    public override async Task SendTestMessages(CancellationToken ct)
    {
        // Send messages to the exchange
        var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        await SendMessage($"All parameters - {date}", new Dictionary<string, object?> {
            {"job", "convert"},
            {"format", "jpeg"}
        }, ct);

        await SendMessage($"Just convert - {date}", new Dictionary<string, object?> {
            {"job", "convert"}
        }, ct);

        await SendMessage($"Just format - {date}", new Dictionary<string, object?> {
            {"format", "jpeg"}
        }, ct);
    }
}
