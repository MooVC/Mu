namespace Mu.Auditing;

using System;

public sealed partial class ScopeManager
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

        return new Lease(current);
    }
}