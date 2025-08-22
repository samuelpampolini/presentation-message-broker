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
        var example = _factory.CreateExample(req.ExampleKey);
        if (example is null)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }
        string result = await example.ExecuteStepAsync(req.Step, ct);
        var response = new ExecuteStepResponse
        {
            Result = result,
            IsComplete = example.IsComplete
        };
        example.Dispose();
        await HttpContext.Response.SendAsync(response, cancellation: ct);
        return;
    }
}
