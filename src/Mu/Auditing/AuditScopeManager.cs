namespace Mu.Auditing;

using System;

public sealed class AuditScopeManager
    : IAuditScopeManager
{
    private static readonly AsyncLocal<State?> _current = new();

    public AuditScope Scope
    {
        get
        {
            State? current = _current.Value;

            if (current is null)
            {
                return AuditScope.External;
            }

            return current.Scope;
        }
    }

    public IDisposable Begin(AuditScope scope)
    {
        State? previous = _current.Value;
        var current = new State(previous, scope);

        _current.Value = current;

        return new Scope(current);
    }

    private sealed record State(State? Previous, AuditScope Scope);

    private sealed class Scope
        : IDisposable
    {
        private readonly State _state;
        private bool _disposed;

        public Scope(State state)
        {
            _state = state;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (ReferenceEquals(_current.Value, _state))
            {
                _current.Value = _state.Previous;
            }

            _disposed = true;
        }
    }
}