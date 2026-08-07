namespace Mu.Modelling.Behavior;

using ProtoBuf;

/// <summary>
/// Represents a request to execute a domain behavior.
/// </summary>
[ProtoContract]
[ProtoInclude(100, typeof(Mutational))]
[ProtoInclude(101, typeof(NonMutational))]
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