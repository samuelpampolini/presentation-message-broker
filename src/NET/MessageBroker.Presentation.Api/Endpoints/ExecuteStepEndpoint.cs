using FastEndpoints;
using MessageBroker.Example.CrossCut.Factories;
using MessageBroker.Example.CrossCut.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Presentation.Api.Endpoints;

public class ExecuteStepRequest
{
    public char ExampleKey { get; set; }
    public ExampleStep Step { get; set; }
}
public class ExecuteStepResponse
{
    public string Result { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
}

public class ExecuteStepEndpoint : Endpoint<ExecuteStepRequest, ExecuteStepResponse>
{
    private readonly ExampleFactory _factory;
    public ExecuteStepEndpoint(ExampleFactory factory) => _factory = factory;
    public override void Configure()
    {
        Post("/examples/execute-step");
        AllowAnonymous();
        Summary(s => s.Summary = "Execute a step for a given example key (per-request stateless execution).");
    }
    public override async Task HandleAsync(ExecuteStepRequest req, CancellationToken ct)
    {
        // Try publisher first
        var pubExample = _factory.CreateExample<ExampleStep>(req.ExampleKey);
        if (pubExample != null)
        {
            string result = await pubExample.ExecuteStepAsync(req.Step, ct);
            var response = new ExecuteStepResponse
            {
                Result = result,
                IsComplete = pubExample.IsComplete
            };
            await HttpContext.Response.SendAsync(response, cancellation: ct);
            return;
        }
        // Try consumer
        var consExample = _factory.CreateExample<ConsumerExampleStep>(req.ExampleKey);
        if (consExample != null)
        {
            // Map ExampleStep to ConsumerExampleStep if possible
            ConsumerExampleStep consStep;
            if (Enum.TryParse(req.Step.ToString(), out consStep))
            {
                string result = await consExample.ExecuteStepAsync(consStep, ct);
                var response = new ExecuteStepResponse
                {
                    Result = result,
                    IsComplete = consExample.IsComplete
                };
                await HttpContext.Response.SendAsync(response, cancellation: ct);
                return;
            }
        }
        await HttpContext.Response.SendNotFoundAsync(ct);
    }
}
