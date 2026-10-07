namespace Mu.Auditing;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Ardalis.GuardClauses;
using MooVC;
using Mu.Communications.Messaging;
using static Mu.Auditing.InMemoryAuditor_Resources;

/// <summary>
/// Stores message audits in memory for development and testing scenarios.
/// </summary>
public sealed partial class InMemoryAuditor
    : IAuditor
{
    private readonly Dictionary<Guid, Entry> _entries = [];
    private readonly Lock _synchronization = new();

    /// <summary>
    /// Gets an immutable snapshot of the captured audit entries.
    /// </summary>
    public ImmutableArray<Entry> Entries
    {
        get
        {
            lock (_synchronization)
            {
                return [.. _entries.Values];
            }
        }
    }

    /// <inheritdoc/>
    public Task<Guid> Capture(Message message, CancellationToken cancellationToken)
    {
        _ = Guard.Against.Null(message, message: CaptureMessageRequired);

        lock (_synchronization)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var identity = Guid.CreateVersion7();

            _entries.Add(identity, new Entry(identity, message));

            return Task.FromResult(identity);
        }
    }

    /// <inheritdoc/>
    public Task Complete<TResult>(Guid identity, Outcome<TResult> outcome, CancellationToken cancellationToken)
        where TResult : notnull
    {
        _ = Guard.Against.Null(outcome, message: CompleteOutcomeRequired);
        _ = Guard.Against.Default(identity, message: UpdateIdentityRequired);

        lock (_synchronization)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Entry entry = GetPendingEntry(identity);

            _entries[identity] = entry with { Outcome = outcome };
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task Fail(Exception cause, Guid identity, CancellationToken cancellationToken)
    {
        _ = Guard.Against.Null(cause, message: FailCauseRequired);
        _ = Guard.Against.Default(identity, message: UpdateIdentityRequired);

        lock (_synchronization)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Entry entry = GetPendingEntry(identity);

            _entries[identity] = entry with { Cause = cause };
        }

        return Task.CompletedTask;
    }

    private Entry GetPendingEntry(Guid identity)
    {
        if (!_entries.TryGetValue(identity, out Entry? entry))
        {
            throw new InvalidOperationException(UpdateIdentityCapturedRequired.Format(identity));
        }

        if (entry.Outcome is not null || entry.Cause is not null)
        {
            throw new InvalidOperationException(UpdateIdentityPendingRequired.Format(identity));
        }

        return entry;
    }
}