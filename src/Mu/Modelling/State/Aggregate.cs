namespace Mu.Modelling.State;

using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Mu.Modelling.Behavior;

public abstract record Aggregate
{
    [JsonIgnore]
    public bool HasChanges => Propositions.Length > 0;

    [JsonPropertyName("$propositions")]
    [JsonInclude]
    internal ImmutableArray<Fact> Propositions { get; init; }
}