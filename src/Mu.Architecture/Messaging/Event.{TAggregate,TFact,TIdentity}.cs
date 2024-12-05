namespace Mu.Architecture.Messaging;

using Mu.Architecture.Modelling;

public sealed record Event<TAggregate, TFact, TIdentity>
    : Event
    where TAggregate : Aggregate<TIdentity>
    where TFact : Fact
    where TIdentity : struct
{
    private static readonly Type type = typeof(TAggregate);

    internal Event(DateTimeOffset committedAt, Context context, TFact fact, Reference<TIdentity> origin, DateTimeOffset preparedAt)
        : base(committedAt, context, fact, preparedAt)
    {
        Fact = fact;
        Origin = origin;
    }

    public new TFact Fact { get; }

    public Reference<TIdentity> Origin { get; }

    public override Type Type => type;
}