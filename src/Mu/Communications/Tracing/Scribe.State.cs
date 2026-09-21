namespace Mu.Communications.Tracing;

/// <summary>
/// Maintains ambient causation and correlation across nested use case scopes.
/// </summary>
/// <remarks>Instances share the current asynchronous execution context. Dispose scopes in reverse order of creation.</remarks>
public partial class Scribe
{
    private sealed record State(Guid? Identity, Ledger Ledger, State? Previous);
}