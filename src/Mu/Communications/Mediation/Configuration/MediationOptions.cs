namespace Mu.Communications.Mediation.Configuration;

/// <summary>
/// Configures the mediator implementation to register.
/// </summary>
public sealed record MediationOptions
{
    /// <summary>
    /// The default mediation options.
    /// </summary>
    public static readonly MediationOptions Default = new();

    /// <summary>
    /// Gets the mediator implementation to register.
    /// </summary>
    public MediatorType Type { get; init; } = MediatorType.InMemory;
}