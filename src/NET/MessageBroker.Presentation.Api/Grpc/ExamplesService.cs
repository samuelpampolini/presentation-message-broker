using Grpc.Core;
using MessageBroker.Presentation.Api.Grpc;
using MessageBroker.Example.CrossCut.Factories;
using MessageBroker.Example.CrossCut.Interfaces;
using System.Threading.Tasks;

namespace MessageBroker.Presentation.Api.Grpc
{
    public class ExamplesService : Examples.ExamplesBase
    {
        private readonly ExampleFactory _factory;
        public ExamplesService(ExampleFactory factory)
        {
            _factory = factory;
        }

        public override async Task<ExecuteStepResponse> ExecuteStep(ExecuteStepRequest request, ServerCallContext context)
        {
            using var example = _factory.CreateExample(request.ExampleKey[0]);
            if (example is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Example not found"));
            }
            if (!System.Enum.TryParse<ExampleStep>(request.Step, out var step))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid step"));
            }
            string result = await example.ExecuteStepAsync(step, context.CancellationToken);
            return new ExecuteStepResponse
            {
                Result = result,
                IsComplete = example.IsComplete
            };
        }
    }
}
