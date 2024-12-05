namespace Mu.Architecture.Modelling;

using System;

public abstract record Fact<TAggregate, TIdentity>(Guid Identity, DateTimeOffset Proposed)
    : Fact(Identity, Proposed)
    where TAggregate : Aggregate<TIdentity>
    where TIdentity : struct
{
    private static readonly Type type = typeof(TAggregate);

    public override Type Type => type;
}