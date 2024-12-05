namespace Mu.Architecture.Persistence;

using Mu.Architecture.Messaging;
using Mu.Architecture.Modelling;

public interface IStream<TIdentity>
    where TIdentity : struct
{
    /// <returns>The time at which the facts are deemed to be committed to the stream.</returns>
    Task<DateTimeOffset> Append(IEnumerable<Fact> facts, TIdentity identity, CancellationToken cancellationToken);

    Task<IReadOnlyList<Event>> Get(CancellationToken cancellationToken, Type[]? facts = default, TIdentity? identity = default);
}