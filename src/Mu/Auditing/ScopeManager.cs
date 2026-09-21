namespace Mu.Auditing;

using System;

public sealed class ScopeManager
    : IScopeManager
{
    private static readonly AsyncLocal<State?> _current = new();

    public Scope Scope
    {
        get
        {
            State? current = _current.Value;

            if (current is null)
            {
                return Scope.External;
            }

            return current.Scope;
        }
    }

    public IDisposable Begin(Scope scope)
    {
        State? previous = _current.Value;
        var current = new State(previous, scope);

        _current.Value = current;

        return new ScopeLease(current);
    }

    private sealed record State(State? Previous, Scope Scope);

    private sealed class ScopeLease
        : IDisposable
    {
        private readonly State _state;
        private bool _disposed;

        public ScopeLease(State state)
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