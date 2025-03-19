namespace Mu.Persistence;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Mu.Communications.Messaging;
using Mu.Modelling.State;

public sealed class WriteStore<TAggregate, TIdentity>(IStream<TIdentity> stream)
    : IWriteStore<TAggregate, TIdentity>
    where TAggregate : Aggregate
    where TIdentity : struct
{
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

        return Task.FromResult<TAggregate?>(default);
    }

    public async Task Save(TAggregate aggregate, TIdentity identity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        if (!aggregate.HasChanges)
        {
            return;
        }

        _ = await stream
            .Append(aggregate.Propositions, identity, cancellationToken)
            .ConfigureAwait(false);
    }
}