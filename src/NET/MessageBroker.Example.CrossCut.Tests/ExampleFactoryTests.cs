using Xunit;
using MessageBroker.Example.CrossCut.Factories;
using System.Collections.Generic;

namespace MessageBroker.Example.CrossCut.Tests
{
    public class ExampleFactoryTests
    {
        [Fact]
        public void GetExamples_ReturnsDictionary()
        {
            // Arrange
            var factory = TestFactoryBuilder.CreateFactoryWithExamples();

            // Act
            var examples = factory.GetExamples();

            // Assert
            Assert.NotNull(examples);
            Assert.True(examples.Count > 0);
        }
    }

    // Helper to create a factory with dummy examples
    public static class TestFactoryBuilder
    {
        public static ExampleFactory CreateFactoryWithExamples()
        {
            var serviceProvider = new DummyServiceProvider();
            return new ExampleFactory(serviceProvider);
        }
    }

    public class DummyServiceProvider : IServiceProvider
    {
        public object? GetService(System.Type serviceType) => null;
    }
    public class DummyInputProvider : MessageBroker.Example.CrossCut.Interfaces.IExampleInputProvider
    {
        public Task<string> GetInputAsync(string prompt, System.Threading.CancellationToken ct) => Task.FromResult("");
    }
    public class DummyOutputHandler : MessageBroker.Example.CrossCut.Interfaces.IExampleOutputHandler
    {
        public Task WriteOutputAsync(string message, System.Threading.CancellationToken ct) => Task.CompletedTask;
        public Task RenderMenuAsync(IReadOnlyDictionary<char, ExampleDetails> examples, System.Threading.CancellationToken ct) => Task.CompletedTask;
        public Task ClearScreenAsync(System.Threading.CancellationToken ct) => Task.CompletedTask;
    }
}
