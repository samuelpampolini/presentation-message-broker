using FastEndpoints;
using MessageBroker.Example.CrossCut.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace MessageBroker.Presentation.Api.Endpoints;

public class ListStepsResponse
{
    public List<StepInfo> Steps { get; set; } = new();
    public class StepInfo
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

public class ListStepsEndpoint : EndpointWithoutRequest<ListStepsResponse>
{
    public override void Configure()
    {
        Get("/steps");
        AllowAnonymous();
        Summary(s => s.Summary = "List all available example steps.");
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var steps = System.Enum.GetValues(typeof(ExampleStep))
            .Cast<ExampleStep>()
            .Select(e => new ListStepsResponse.StepInfo { Value = (int)e, Name = e.ToString() })
            .ToList();
        var response = new ListStepsResponse { Steps = steps };
        await HttpContext.Response.SendAsync(response, cancellation: ct);
    }
}
