namespace MessageBroker.Example.CrossCut.Interfaces;


/// <summary>
/// Steps for publisher examples.
/// </summary>
public enum ExampleStep
{
    Setup = 1,
    SendMessages = 2,
    CleanUp = 3
}

/// <summary>
/// Steps for consumer examples (no cleanup, explicit consume step).
/// </summary>
public enum ConsumerExampleStep
{
    Setup = 1,
    SendMessages = 2,
    ConsumeMessages = 3
}


public interface IMessageExample<TStep> : IDisposable where TStep : Enum
{
    /// <summary>
    /// Executes the specified step of the example.
    /// </summary>
    Task<string> ExecuteStepAsync(TStep step, CancellationToken ct);

    /// <summary>
    /// Returns true if the example is finished.
    /// </summary>
    bool IsComplete { get; }
}
