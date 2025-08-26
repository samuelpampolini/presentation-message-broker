using FastEndpoints;
using MessageBroker.Example.CrossCut.Factories;
using MessageBroker.Example.CrossCut.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Presentation.Api.Endpoints;

public class StartConsumerRequest
{
    public char ExampleKey { get; set; }
}

public class StartConsumerEndpoint : Endpoint<StartConsumerRequest>
{
    private readonly ExampleFactory _factory;
    public StartConsumerEndpoint(ExampleFactory factory) => _factory = factory;

    public override void Configure()
    {
        Post("/examples/start-consumer");
        AllowAnonymous();
        Summary(s => s.Summary = "Start consuming messages for a given consumer example key using ExecuteStep.");
    }

    public override async Task HandleAsync(StartConsumerRequest req, CancellationToken ct)
    {
        var consExample = _factory.CreateExample<ConsumerExampleStep>(req.ExampleKey);
        if (consExample != null)
        {
            // Use ExecuteStepAsync to emulate frontend behavior
            _ = consExample.ExecuteStepAsync(ConsumerExampleStep.ConsumeMessages, ct);
            await HttpContext.Response.SendOkAsync(ct);
            return;
        }
        await HttpContext.Response.SendNotFoundAsync(ct);
    }
}
