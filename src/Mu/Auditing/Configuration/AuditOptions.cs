namespace Mu.Auditing.Configuration;

public sealed record AuditOptions
{
    public static readonly AuditOptions Default = new();

    public AuditOperationScope Mutational { get; init; } = AuditOperationScope.All;

    public AuditOperationScope NonMutational { get; init; } = AuditOperationScope.External;

    public AuditorType Type { get; init; } = AuditorType.InMemory;
}