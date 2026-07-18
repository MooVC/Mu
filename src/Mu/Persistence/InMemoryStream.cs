namespace Mu.Persistence;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Mu.Communications.Messaging;
using Mu.Communications.Tracing;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;

/// <summary>
/// Stores aggregate events in memory for development and testing scenarios.
/// </summary>
internal sealed class InMemoryStream<TIdentity>
    : IStream<TIdentity>
    where TIdentity : struct
{
    private readonly List<StoredEvent> _events = [];
    private readonly Dictionary<TIdentity, ulong> _revisions = [];
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// Initializes a <see langword="new"/> instance of the <see cref="InMemoryStream{TIdentity}"/> class.
    /// </summary>
    internal InMemoryStream()
        : this(new SemaphoreSlim(1, 1))
    {
    }

    /// <summary>
    /// Initializes a <see langword="new"/> instance of the <see cref="InMemoryStream{TIdentity}"/> class.
    /// </summary>
    /// <param name="semaphore">The semaphore used to synchronize access to the stream.</param>
    internal InMemoryStream(SemaphoreSlim semaphore)
    {
        _semaphore = Guard.Against.Null(semaphore, message: "The semaphore used to synchronize access to the stream must be provided.");
    }

    /// <summary>
    /// Appends facts for an existing in-memory stream.
    /// </summary>
    public async Task<DateTimeOffset> Append(IEnumerable<Fact> facts, TIdentity identity, Revision revision, CancellationToken cancellationToken)
    {
        _ = Guard.Against.Null(facts, message: $"The facts to be appended to the stream for `{identity}` at revision `{revision.Number}` must be provided.");
        _ = Guard.Against.Default(identity, message: $"The identity of the stream to which the facts are to be appended must be provided.");

        ImmutableArray<Fact> buffered = [.. facts];

        if (buffered.Length == 0)
        {
            return DateTimeOffset.UtcNow;
        }

        await _semaphore
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            if (!_revisions.TryGetValue(identity, out ulong current))
            {
                throw new InvalidOperationException($"The stream for identity `{identity}` has not been initiated.");
            }

            if (current != revision.Number)
            {
                throw new InvalidOperationException(
                    $"The stream for identity `{identity}` is at revision `{current}`, not `{revision.Number}`.");
            }

            return Write(buffered, identity, current);
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    /// <summary>
    /// Finds events in the in-memory stream by using the provided options.
    /// </summary>
    public async Task<ImmutableArray<Event>> Find(IStream<TIdentity>.FindOptions options, CancellationToken cancellationToken)
    {
        _ = Guard.Against.Null(options, message: "The options for finding events in the stream must be provided.");

        await _semaphore
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            IEnumerable<StoredEvent> matches = _events;

            if (!options.Facts.IsDefaultOrEmpty)
            {
                matches = matches.Where(stored => options.Facts.Contains(stored.Event.Fact.GetType()));
            }

            if (options.Committed != default)
            {
                matches = matches.Where(stored => IsWithin(stored.Event.CommittedAt, options.Committed));
            }

            if (options.Proposed != default)
            {
                matches = matches.Where(stored => IsWithin(stored.Event.Fact.Proposed, options.Proposed));
            }

            if (options.Revision != default)
            {
                matches = matches.Where(stored => IsWithin(stored.Revision, options.Revision));
            }

            if (!EqualityComparer<TIdentity>.Default.Equals(options.Identity, default))
            {
                matches = matches.Where(stored => EqualityComparer<TIdentity>.Default.Equals(stored.Identity, options.Identity));
            }

            return [.. matches.Select(stored => stored.Event)];
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    /// <summary>
    /// Initiates a <see langword="new"/> in-memory stream with initial facts.
    /// </summary>
    /// <param name="facts">The facts to initialize the stream with.</param>
    /// <param name="identity">The identity of the stream to initiate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task<DateTimeOffset> Initiate(IEnumerable<Fact> facts, TIdentity identity, CancellationToken cancellationToken)
    {
        _ = Guard.Against.Null(facts, message: $"The facts to initialize the stream for `{identity}` must be provided.");
        _ = Guard.Against.Default(identity, message: $"The identity of the stream to initiate for `{identity}` must be provided.");

        ImmutableArray<Fact> buffered = [.. facts];

        await _semaphore
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            if (_revisions.ContainsKey(identity))
            {
                throw new InvalidOperationException($"The stream for identity `{identity}` has already been initiated.");
            }

            return Write(buffered, identity, 0);
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    private static Event CreateEvent(DateTimeOffset committedAt, Fact fact, Reference<TIdentity> origin)
    {
        return CreateTypedEvent(committedAt, (dynamic)fact, origin);
    }

    private static Event<TFact, TIdentity> CreateTypedEvent<TFact>(DateTimeOffset committedAt, TFact fact, Reference<TIdentity> origin)
        where TFact : Fact
    {
        var ledger = new Ledger(fact.Identity);

        return new Event<TFact, TIdentity>(committedAt, ledger, fact, origin, committedAt);
    }

    private static bool IsWithin<T>(T value, Range<T> range)
        where T : IComparable<T>
    {
        return value.CompareTo(range.From) >= 0 && value.CompareTo(range.To) <= 0;
    }

    private DateTimeOffset Write(ImmutableArray<Fact> facts, TIdentity identity, ulong revision)
    {
        DateTimeOffset committedAt = DateTimeOffset.UtcNow;
        var events = new List<StoredEvent>(facts.Length);

        for (int index = 0; index < facts.Length; index++)
        {
            Fact fact = facts[index];

            _ = Guard.Against.Null(fact, message: $"The fact at index `{index}` for the stream `{identity}` at revision `{revision}` cannot be null.");

            revision = checked(revision + 1);

            var origin = new Reference<TIdentity>(identity, revision);
            Event @event = CreateEvent(committedAt, fact, origin);

            events.Add(new StoredEvent(@event, identity, revision));
        }

        _events.AddRange(events);
        _revisions[identity] = revision;

        return committedAt;
    }

    private sealed record StoredEvent(Event Event, TIdentity Identity, ulong Revision);
}