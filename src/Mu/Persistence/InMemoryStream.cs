namespace Mu.Persistence;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;

/// <summary>
/// In-memory stream placeholder implementation for development and testing scenarios.
/// </summary>
internal sealed class InMemoryStream<TIdentity>
    : IStream<TIdentity>
    where TIdentity : struct
{
    /// <summary>
    /// Appends facts for an existing in-memory stream.
    /// </summary>
    public Task<DateTimeOffset> Append(IEnumerable<Fact> facts, TIdentity identity, Revision revision, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Finds events in the in-memory stream by using the provided options.
    /// </summary>
    public Task<ImmutableArray<Event>> Find(IStream<TIdentity>.FindOptions options, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Initiates a new in-memory stream with initial facts.
    /// </summary>
    public Task<DateTimeOffset> Initiate(IEnumerable<Fact> facts, TIdentity identity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
