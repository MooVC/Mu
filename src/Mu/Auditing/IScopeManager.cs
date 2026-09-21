namespace Mu.Auditing;

public interface IScopeManager
{
    Scope Scope { get; }

    IDisposable Begin(Scope scope);
}