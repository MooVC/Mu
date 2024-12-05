namespace Mu.Architecture.Modelling;

public abstract record Fact(Guid Identity, DateTimeOffset Proposed)
    : Causal(Identity, Proposed)
{
}