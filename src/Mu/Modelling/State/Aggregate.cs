namespace Mu.Modelling.State;

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Mu.Modelling.Behavior;
using ProtoBuf;

/// <summary>
/// Base immutable representation of aggregate state and proposed facts.
/// </summary>
[ProtoContract]
public abstract record Aggregate
{
    /// <summary>
    /// Gets the <see cref="Representation"/> for the aggregate type.
    /// </summary>
    public static readonly Representation Representation = typeof(Aggregate);

    /// <summary>
    /// Gets a value indicating whether the aggregate has pending propositions.
    /// </summary>
    [JsonIgnore]
    [ProtoIgnore]
    internal bool HasChanges => Propositions.Length > 0;

    /// <summary>
    /// Gets the facts proposed since the aggregate was loaded.
    /// </summary>
    [JsonPropertyName("$propositions")]
    [JsonInclude]
    [ProtoMember(1, Name = nameof(Propositions))]
    internal ImmutableArray<Fact> Propositions { get; init; } = [];

    /// <summary>
    /// Gets the current revision for the aggregate.
    /// </summary>
    [ProtoMember(2, Name = nameof(Revision))]
    internal Revision Revision { get; init; }
}