namespace MessageBroker.Example.CrossCut.Interfaces;

public enum ExampleStep
{
    Setup = 1,
    SendMessages = 2,
    CleanUp = 3
}

public interface IMessageExample : IDisposable
{
    /// <summary>
    /// Executes the specified step of the example.
    /// </summary>
    Task<string> ExecuteStepAsync(ExampleStep step, CancellationToken ct);

    /// <summary>
    /// Returns true if the example is finished.
    /// </summary>
    bool IsComplete { get; }
}
