namespace Mu.Communications.Tracing;

using Mu.Modelling.Behavior;

/// <summary>
/// Manages ambient trace ledger scope for nested use case execution.
/// </summary>
public sealed class Scope
    : IDisposable
{
    private static readonly AsyncLocal<Ledger?> _current = new();
    private readonly Ledger? _previous;

    /// <summary>
    /// Initializes a new tracing scope for a use case.
    /// </summary>
    public Scope(UseCase useCase)
        : this(GetLedger(useCase))
    {
    }

    /// <summary>
    /// Initializes a new tracing scope with an explicit ledger.
    /// </summary>
    public Scope(Ledger ledger)
    {
        _previous = _current.Value;
        _current.Value = Ledger = ledger;
    }

    /// <summary>
    /// Gets the ledger active for this scope.
    /// </summary>
    public Ledger Ledger { get; }

    /// <summary>
    /// Restores the previous ambient ledger.
    /// </summary>
    public void Dispose()
    {
        _current.Value = _previous;
    }

    private static Ledger GetLedger(UseCase useCase)
    {
        ArgumentNullException.ThrowIfNull(useCase);

        Ledger? current = _current.Value;

        if (current.HasValue)
        {
            return current.Value.Next(useCase.Identity);
        }

        return new Ledger(useCase.Identity);
    }
}
