namespace Mu.Persistence;

using Mu.Modelling.State;

/// <summary>
/// Provides aggregate persistence operations backed by an event stream.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type being stored.</typeparam>
/// <typeparam name="TIdentity">The aggregate identity type.</typeparam>
public interface IWriteStore<TAggregate, TIdentity>
    where TAggregate : Aggregate
    where TIdentity : struct
{
    /// <summary>
    /// Rehydrates an aggregate instance at the requested revision.
    /// </summary>
    Task<TAggregate?> Get(TIdentity identity, ulong revision, CancellationToken cancellationToken);

    /// <summary>
    /// Persists proposed aggregate facts to the stream.
    /// </summary>
    Task Save(TAggregate aggregate, TIdentity identity, CancellationToken cancellationToken);
}