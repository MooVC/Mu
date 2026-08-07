namespace Mu.Modelling.Behavior;

using ProtoBuf;

/// <summary>
/// Represents a non-mutational query use case.
/// </summary>
[ProtoContract]
public abstract record Query
    : NonMutational
{
    private protected Query()
    {
    }

    private protected Query(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}