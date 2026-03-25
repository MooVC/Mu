namespace Mu.Persistence;

using System.Collections.Immutable;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;

/// <summary>
/// Defines the append-only event stream for aggregate facts.
/// </summary>
/// <typeparam name="TIdentity">The aggregate identity type.</typeparam>
public interface IStream<TIdentity>
    where TIdentity : struct
{
    /// <summary>
    /// Appends facts for an existing aggregate revision.
    /// </summary>
    /// <returns>The time at which the facts are deemed to be committed to the stream.</returns>
    Task<DateTimeOffset> Append(IEnumerable<Fact> facts, TIdentity identity, Revision revision, CancellationToken cancellationToken);

    /// <summary>
    /// Initiates a new aggregate stream with its initial facts.
    /// </summary>
    /// <returns>The time at which the facts are deemed to be committed to the stream.</returns>
    Task<DateTimeOffset> Initiate(IEnumerable<Fact> facts, TIdentity identity, CancellationToken cancellationToken);

    /// <summary>
    /// Finds events in the stream using the provided filter options.
    /// </summary>
    Task<ImmutableArray<Event>> Find(FindOptions options, CancellationToken cancellationToken);

    /// <summary>
    /// Provides filtering options for stream queries.
    /// </summary>
    public sealed record FindOptions(
        ImmutableArray<Type> Facts = default,
        Range<DateTimeOffset> Committed = default,
        Range<DateTimeOffset> Proposed = default,
        Range<ulong> Revision = default,
        TIdentity Identity = default);
}
