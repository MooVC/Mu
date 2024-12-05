namespace Mu.Architecture.Modelling;

public abstract record Query<TAggregate, TIdentity>
    : Query
    where TAggregate : Aggregate<TIdentity>
    where TIdentity : struct
{
    private static readonly Type type = typeof(TAggregate);

    protected Query()
    {
    }

    protected Query(DateTimeOffset proposed)
        : base(proposed)
    {
    }

    protected Query(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }

    public override Type Type => type;
}