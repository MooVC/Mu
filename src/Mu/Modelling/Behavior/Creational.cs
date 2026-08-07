namespace Mu.Modelling.Behavior;

using ProtoBuf;

/// <summary>
/// Represents a mutational use case that creates a <see langword="new"/> aggregate instance.
/// </summary>
[ProtoContract]
public abstract record Creational
    : Mutational
{
    private protected Creational()
    {
    }

    private protected Creational(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}