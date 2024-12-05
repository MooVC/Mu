namespace Mu.Architecture.Modelling;

public abstract class Aggregate<TIdentity>
    where TIdentity : struct
{
    private readonly List<Fact> propositions = [];

    protected Aggregate(Func<Fact> creation, Func<TIdentity> identity)
    {
        ArgumentNullException.ThrowIfNull(creation);
        ArgumentNullException.ThrowIfNull(identity);

        Propose(creation);

        Identity = identity();
    }

    protected Aggregate(TIdentity identity, Revision revision, params IEnumerable<Fact> facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        Identity = identity;
        Revision = revision;

        Replay(facts.ToArray());
    }

    public bool HasChanges => propositions.Count > 0;

    public TIdentity Identity { get; }

    public IReadOnlyList<Fact> Propositions => propositions.AsReadOnly();

    public Revision Revision { get; private set; }

    protected abstract void Project(Fact fact);

    protected void Propose(Func<Fact> factory)
    {
        if (!HasChanges)
        {
            Revision++;
        }

        Fact fact = factory();

        propositions.Add(fact);
    }

    private void Replay(Fact[] facts)
    {
        if (!AllOriginatedFromThisAggregateType(facts))
        {
            throw new ArgumentException(nameof(facts));
        }

        DateTimeOffset current = DateTimeOffset.MinValue;

        foreach (Fact fact in facts)
        {
            if (fact.Proposed < current)
            {
                throw new ArgumentException(nameof(facts));
            }

            Project(fact);

            current = fact.Proposed;
        }
    }

    private bool AllOriginatedFromThisAggregateType(Fact[] facts)
    {
        Type type = GetType();

        return Array.TrueForAll(facts, fact => fact.Type == type);
    }
}