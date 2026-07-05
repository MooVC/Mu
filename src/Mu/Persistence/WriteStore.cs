namespace Mu.Persistence;

using System.Collections.Immutable;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using Mu.Modelling.Services;
using Mu.Modelling.State;

/// <summary>
/// Persists aggregate changes by writing proposed facts to an event stream.
/// </summary>
public sealed class WriteStore<TAggregate, TIdentity>(IStream<TIdentity> stream, ITransform<TAggregate, Fact> transform)
    : IWriteStore<TAggregate, TIdentity>
    where TAggregate : Aggregate, new()
    where TIdentity : struct
{
    /// <summary>
    /// Rehydrates an aggregate from persisted events up to the requested revision.
    /// </summary>
    public async Task<TAggregate?> Get(TIdentity identity, ulong revision, CancellationToken cancellationToken)
    {
        ImmutableArray<Event> events = await stream
            .Find(
                new()
                {
                    Identity = identity,
                    Revision = (From: 1ul, To: revision),
                },
                cancellationToken)
            .ConfigureAwait(false);

        if (events.Length == 0)
        {
            return default;
        }

        IEnumerable<Fact> facts = events.Select(@event => @event.Fact);
        TAggregate aggregate = new();

        aggregate = transform.ApplyAll(aggregate, facts);

        return aggregate;
    }

    /// <summary>
    /// Saves aggregate propositions as stream events.
    /// </summary>
    public async Task Save(TAggregate aggregate, TIdentity identity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        if (!aggregate.HasChanges)
        {
            return;
        }

        Task<DateTimeOffset> write = aggregate.Revision.Number == 0
            ? stream.Initiate(aggregate.Propositions, identity, cancellationToken)
            : stream.Append(aggregate.Propositions, identity, aggregate.Revision, cancellationToken);

        _ = await write.ConfigureAwait(false);
    }
}