namespace Mu.Modelling.Behavior;

/// <summary>
/// Represents a request to execute a domain behavior.
/// </summary>
public abstract record UseCase
    : Causal
{
    private protected UseCase()
    {
    }

    private protected UseCase(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}
