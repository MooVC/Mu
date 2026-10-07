namespace Mu.Auditing;

using System;
using Mu.Communications.Messaging;

public sealed partial class InMemoryAuditor
{
    /// <summary>
    /// Represents a captured message and its audit outcome or failure.
    /// </summary>
    /// <param name="Identity">The identifier returned when the message was captured.</param>
    /// <param name="Message">The captured message.</param>
    /// <param name="Outcome">The outcome recorded on completion, or <see langword="null"/>.</param>
    /// <param name="Cause">The exception recorded on failure, or <see langword="null"/>.</param>
    public sealed record Entry(Guid Identity, Message Message, Message? Outcome = default, Exception? Cause = default);
}