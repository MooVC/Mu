namespace Mu.Modelling.State;

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Mu.Modelling.Behavior;

/// <summary>
/// Base immutable representation of aggregate state and proposed facts.
/// </summary>
public abstract record Aggregate
{
    /// <summary>
    /// Gets a value indicating whether the aggregate has pending propositions.
    /// </summary>
    [JsonIgnore]
    internal bool HasChanges => Propositions.Length > 0;

    /// <summary>
    /// Gets the facts proposed since the aggregate was loaded.
    /// </summary>
    [JsonPropertyName("$propositions")]
    [JsonInclude]
    internal ImmutableArray<Fact> Propositions { get; init; }

    /// <summary>
    /// Gets the current revision for the aggregate.
    /// </summary>
    internal Revision Revision { get; init; }
}