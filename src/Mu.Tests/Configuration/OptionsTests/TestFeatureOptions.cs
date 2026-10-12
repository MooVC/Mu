namespace Mu.Testing;

using Mu.Configuration;

public sealed record TestFeatureOptions
    : Options
{
    public string Description { get; init; } = string.Empty;
}