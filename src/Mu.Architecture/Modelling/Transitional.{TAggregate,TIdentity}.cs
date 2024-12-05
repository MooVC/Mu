namespace Mu.Architecture.Modelling;

public abstract record Transitional<TAggregate, TIdentity>
    : Transitional
    where TAggregate : Aggregate<TIdentity>
    where TIdentity : struct
{
    private static readonly Type type = typeof(TAggregate);

    protected Transitional()
    {
    }

    protected Transitional(DateTimeOffset proposed)
        : base(proposed)
    {
    }

    protected Transitional(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }

    public override Type Type => type;
}