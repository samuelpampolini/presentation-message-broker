using FastEndpoints;
using MessageBroker.Example.CrossCut.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace MessageBroker.Presentation.Api.Endpoints;


public class ListStepsResponse
{
    public List<StepGroup> StepGroups { get; set; } = new();
    public class StepGroup
    {
        public string Type { get; set; } = string.Empty; // "publisher" or "consumer"
        public List<StepInfo> Steps { get; set; } = new();
    }
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
        var publisherSteps = System.Enum.GetValues(typeof(ExampleStep))
            .Cast<ExampleStep>()
            .Select(e => new ListStepsResponse.StepInfo { Value = (int)e, Name = e.ToString() })
            .ToList();
        var consumerSteps = System.Enum.GetValues(typeof(ConsumerExampleStep))
            .Cast<ConsumerExampleStep>()
            .Select(e => new ListStepsResponse.StepInfo { Value = (int)e, Name = e.ToString() })
            .ToList();
        var response = new ListStepsResponse
        {
            StepGroups = new List<ListStepsResponse.StepGroup>
            {
                new ListStepsResponse.StepGroup { Type = "publisher", Steps = publisherSteps },
                new ListStepsResponse.StepGroup { Type = "consumer", Steps = consumerSteps }
            }
        };
        await HttpContext.Response.SendAsync(response, cancellation: ct);
    }
}
