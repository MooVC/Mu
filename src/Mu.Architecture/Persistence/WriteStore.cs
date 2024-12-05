namespace Mu.Architecture.Persistence;

using Mu.Architecture.Modelling;

public sealed class WriteStore<TAggregate, TIdentity>(IStream<TIdentity> stream)
    : IWriteStore<TAggregate, TIdentity>
    where TAggregate : Aggregate<TIdentity>
    where TIdentity : struct
{
    public Task<TAggregate?> Get(TIdentity identity, ulong revision, CancellationToken cancellationToken)
    {
        return Task.FromResult<TAggregate?>(default);
    }

    public async Task Save(CancellationToken cancellationToken, params IEnumerable<TAggregate> aggregates)
    {
        foreach (TAggregate aggregate in aggregates)
        {
            if (aggregate.HasChanges)
            {
                continue;
            }

            _ = await stream
                .Append(aggregate.Propositions, aggregate.Identity, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}