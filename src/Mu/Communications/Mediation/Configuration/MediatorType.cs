namespace Mu.Communications.Mediation.Configuration;

/// <summary>
/// Identifies the mediator implementation to register.
/// </summary>
public enum MediatorType
{
    /// <summary>
    /// Resolves and executes handlers in memory.
    /// </summary>
    InMemory = 0,
}