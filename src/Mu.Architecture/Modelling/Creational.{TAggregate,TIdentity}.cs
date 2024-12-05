namespace Mu.Architecture.Modelling;

public abstract record Creational<TAggregate, TIdentity>
    : Creational
    where TAggregate : Aggregate<TIdentity>
    where TIdentity : struct
{
    private static readonly Type type = typeof(TAggregate);

    protected Creational()
    {
    }

    protected Creational(DateTimeOffset proposed)
        : base(proposed)
    {
    }

    protected Creational(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }

    public override Type Type => type;
}