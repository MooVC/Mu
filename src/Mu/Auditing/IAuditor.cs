namespace Mu.Auditing;

using System;
using Mu.Communications.Messaging;

/// <summary>
/// Defines the contract for an auditor that captures and completes auditing of messages.
/// </summary>
public interface IAuditor
{
    /// <summary>
    /// Captures a message for auditing purposes.
    /// </summary>
    /// <param name="message">The message to capture.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The identifier for the captured message to be used on completion.</returns>
    Task<Guid> Capture(Message message, CancellationToken cancellationToken);

    /// <summary>
    /// Completes the auditing process for the specified outcome.
    /// </summary>
    /// <param name="identity">The identifier of the captured message.</param>
    /// <param name="outcome">The outcome to complete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Complete<TResult>(Guid identity, Outcome<TResult> outcome, CancellationToken cancellationToken)
        where TResult : notnull;

    /// <summary>
    /// Marks the auditing process as failed for the specified identity, providing the cause of failure.
    /// </summary>
    /// <param name="cause">The cause of the failure.</param>
    /// <param name="identity">The identifier of the captured message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Fail(Exception cause, Guid identity, CancellationToken cancellationToken);
}