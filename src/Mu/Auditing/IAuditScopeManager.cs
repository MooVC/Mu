namespace Mu.Auditing;

public interface IAuditScopeManager
{
    AuditScope Scope { get; }

    IDisposable Begin(AuditScope scope);
}