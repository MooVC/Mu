namespace Mu.Communications.Tracing;

using Ardalis.GuardClauses;
using static Mu.Communications.Tracing.Scribe_Resources;

/// <summary>
/// Maintains ambient causation and correlation across nested use case scopes.
/// </summary>
/// <remarks>Instances share the current asynchronous execution context. Dispose scopes in reverse order of creation.</remarks>
public partial class Scribe
{
    private sealed class Lease
        : IDisposable
    {
        private readonly State _state;

        public Lease(State state)
        {
            _state = state;
        }

        public void Dispose()
        {
            for (State? current = _current.Value; current is not null; current = current.Previous)
            {
                if (ReferenceEquals(current, _state))
                {
                    _ = Guard.Against.InvalidInput(
                        current,
                        nameof(Lease),
                        state => ReferenceEquals(_current.Value, state),
                        message: DisposeScopeOrderRequired,
                        exceptionCreator: () => new InvalidOperationException(DisposeScopeOrderRequired));

                    _current.Value = _state.Previous;

                    return;
                }
            }
        }
    }
}