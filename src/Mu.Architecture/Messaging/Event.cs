namespace Mu.Architecture.Messaging;

using Mu.Architecture.Modelling;

public abstract record Event
    : Message
{
    private protected Event(DateTimeOffset committedAt, Context context, Fact fact, DateTimeOffset preparedAt)
        : base(context, preparedAt)
    {
        ArgumentNullException.ThrowIfNull(fact);

        CommittedAt = committedAt;
        Fact = fact;
    }

    public DateTimeOffset CommittedAt { get; }

    public Fact Fact { get; }

    public abstract Type Type { get; }
}