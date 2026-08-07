namespace Mu.Modelling.Behavior;

using ProtoBuf;

/// <summary>
/// Represents a use case that mutates aggregate state.
/// </summary>
[ProtoContract]
[ProtoInclude(100, typeof(Creational))]
[ProtoInclude(101, typeof(Transitional))]
public abstract record Mutational
    : UseCase
{
    private protected Mutational()
    {
    }

    private protected Mutational(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}