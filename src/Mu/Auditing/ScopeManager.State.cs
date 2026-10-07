namespace Mu.Auditing;

public partial class ScopeManager
{
    private sealed record State(State? Previous, Scope Scope);
}