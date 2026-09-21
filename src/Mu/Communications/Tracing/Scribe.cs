namespace Mu.Communications.Tracing;

using Ardalis.GuardClauses;
using Mu.Modelling.Behavior;
using static Mu.Communications.Tracing.Scribe_Resources;

/// <summary>
/// Maintains ambient causation and correlation across nested use case scopes.
/// </summary>
/// <remarks>Instances share the current asynchronous execution context. Dispose scopes in reverse order of creation.</remarks>
public sealed partial class Scribe
    : IScribe
{
    private static readonly AsyncLocal<State?> _current = new();

    /// <inheritdoc/>
    public Ledger Ledger => Guard.Against.Null(
        _current.Value,
        message: LedgerScopeRequired,
        exceptionCreator: () => new InvalidOperationException(LedgerScopeRequired)).Ledger;

    /// <inheritdoc/>
    public IDisposable Next(UseCase useCase)
    {
        _ = Guard.Against.Null(useCase, message: NextUseCaseRequired);

        State? previous = _current.Value;
        Ledger ledger = previous?.Ledger ?? new Ledger(useCase.Identity);

        if (previous?.Identity is Guid identity)
        {
            ledger = ledger.Next(identity);
        }

        return Begin(useCase.Identity, ledger);
    }

    /// <inheritdoc/>
    public IDisposable Set(Ledger ledger)
    {
        _ = Guard.Against.InvalidInput(
            ledger,
            nameof(ledger),
            _ => _current.Value is null,
            message: SetLedgerAlreadyEstablished,
            exceptionCreator: () => new InvalidOperationException(SetLedgerAlreadyEstablished));

        return Begin(default, ledger);
    }

    private static Lease Begin(Guid? identity, Ledger ledger)
    {
        var current = new State(identity, ledger, _current.Value);
        _current.Value = current;

        return new Lease(current);
    }
}