namespace Mu.Architecture.Persistence;

using Mu.Architecture.Modelling;

public interface IWriteStore<TAggregate, TIdentity>
    where TAggregate : Aggregate<TIdentity>
    where TIdentity : struct
{
    Task<TAggregate?> Get(TIdentity identity, ulong revision, CancellationToken cancellationToken);

    Task Save(CancellationToken cancellationToken, params IEnumerable<TAggregate> aggregates);
}