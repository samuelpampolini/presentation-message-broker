using Xunit;
using MessageBroker.Example.CrossCut.Factories;
using MessageBroker.Example.CrossCut.Interfaces;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace MessageBroker.Example.CrossCut.Tests
{
    public class ExampleFactoryInteractiveTests
    {
        [Fact]
        public async Task StartTests_ValidKeyAndEscape_ExecutesSteps()
        {
            // Arrange: Setup input sequence (valid key, then Escape)
            var inputSequence = new Queue<string>(new[] { "1", "Escape" });
            var inputProvider = new SequenceInputProvider(inputSequence);
            var outputHandler = new RecordingOutputHandler();
            var serviceProvider = new DummyServiceProvider();
            var factory = new ExampleFactory(serviceProvider, inputProvider, outputHandler);

            // Add a dummy example to the factory
            var dummyType = typeof(DummyExample);
            var details = new ExampleDetails("Dummy Example", dummyType);
            var examplesField = typeof(ExampleFactory).GetField("_examples", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            examplesField.SetValue(factory, new SortedDictionary<char, ExampleDetails> { { '1', details } }.ToImmutableSortedDictionary());

            // Act
            await factory.StartTests();

            // Assert: Check output steps
            Assert.Contains(outputHandler.Outputs, m => m.Contains("Available Examples"));
            Assert.Contains(outputHandler.Outputs, m => m.Contains("Creating example of type Dummy Example"));
            Assert.Contains(outputHandler.Outputs, m => m.Contains("Press any key to continue"));
        }

        // Dummy implementations for testing
        public class SequenceInputProvider : IExampleInputProvider
        {
            private readonly Queue<string> _inputs;
            public SequenceInputProvider(Queue<string> inputs) => _inputs = inputs;
            public Task<string> GetInputAsync(string prompt, CancellationToken ct) => Task.FromResult(_inputs.Count > 0 ? _inputs.Dequeue() : "Escape");
        }
        public class RecordingOutputHandler : IExampleOutputHandler
        {
            public List<string> Outputs { get; } = new();
            public Task WriteOutputAsync(string message, CancellationToken ct) { Outputs.Add(message); return Task.CompletedTask; }
            public Task RenderMenuAsync(IReadOnlyDictionary<char, ExampleDetails> examples, CancellationToken ct) { Outputs.Add("Available Examples"); return Task.CompletedTask; }
            public Task ClearScreenAsync(CancellationToken ct) { Outputs.Add("Screen Cleared"); return Task.CompletedTask; }
        }
        public class DummyServiceProvider : IServiceProvider
        {
            public object? GetService(System.Type serviceType) => serviceType == typeof(DummyExample) ? new DummyExample() : null;
        }
        public class DummyExample : IMessageExample
        {
            public Task RunExample(CancellationToken ct) => Task.CompletedTask;
            public void Dispose() { }
        }
    }
}
