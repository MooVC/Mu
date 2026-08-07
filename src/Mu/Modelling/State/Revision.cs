namespace Mu.Modelling.State;

using System.Text.Json.Serialization;
using ProtoBuf;

/// <summary>
/// Represents the revision metadata for an aggregate stream.
/// </summary>
[ProtoContract]
public readonly record struct Revision
{
    /// <summary>
    /// Initializes an unspecified revision.
    /// </summary>
    public Revision()
        : this(DateTimeOffset.MinValue, 0)
    {
    }

    /// <summary>
    /// Initializes a <see langword="new"/> instance of the <see cref="Revision"/> record.
    /// </summary>
    [JsonConstructor]
    internal Revision(DateTimeOffset initiatedAt, ulong number)
    {
        InitiatedAt = initiatedAt;
        Number = number;
    }

    /// <summary>
    /// Gets the initiation timestamp for the revision.
    /// </summary>
    [ProtoMember(1, Name = nameof(InitiatedAt))]
    public DateTimeOffset InitiatedAt { get; }

    /// <summary>
    /// Gets the monotonically increasing revision number.
    /// </summary>
    [ProtoMember(2, Name = nameof(Number))]
    public ulong Number { get; }

    /// <summary>
    /// Increments the revision number and updates the initiation timestamp.
    /// </summary>
    public static Revision operator ++(Revision revision)
    {
        checked
        {
            return new Revision(DateTimeOffset.UtcNow, revision.Number + 1);
        }
    }
}