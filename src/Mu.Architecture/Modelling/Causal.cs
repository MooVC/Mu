namespace Mu.Architecture.Modelling;

public abstract record Causal
{
    private protected Causal()
        : this(Guid.Empty, DateTimeOffset.UtcNow)
    {
    }

    private protected Causal(DateTimeOffset proposed)
        : this(Guid.CreateVersion7(proposed), proposed)
    {
    }

    private protected Causal(Guid identity, DateTimeOffset proposed)
    {
        Identity = identity;
        Proposed = proposed;
    }

    public Guid Identity { get; }

    public DateTimeOffset Proposed { get; }

    public abstract Type Type { get; }
}