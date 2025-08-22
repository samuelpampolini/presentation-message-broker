using FastEndpoints;
using MessageBroker.Example.CrossCut.Factories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Presentation.Api.Endpoints;

public class ListExamplesRequest { }
public class ListExamplesResponse
{
    public List<ExampleInfo> Examples { get; set; } = new();
    public class ExampleInfo
    {
        public char Key { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}

public class ListExamplesEndpoint : EndpointWithoutRequest<ListExamplesResponse>
{
    private readonly ExampleFactory _factory;
    public ListExamplesEndpoint(ExampleFactory factory) => _factory = factory;
    public override void Configure()
    {
        Get("/examples");
        AllowAnonymous();
        Summary(s => s.Summary = "List all available examples.");
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new ListExamplesResponse
        {
            Examples = new List<ListExamplesResponse.ExampleInfo>()
        };
        foreach (var kv in _factory.GetExamples())
        {
            response.Examples.Add(new ListExamplesResponse.ExampleInfo
            {
                Key = kv.Key,
                Title = kv.Value.title
            });
        }
        await HttpContext.Response.SendAsync(response, cancellation: ct);
        return;
    }
}
