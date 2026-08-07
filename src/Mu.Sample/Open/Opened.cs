namespace Mu.Sample.Open;

using System.Text.Json.Serialization;
using Mu.Modelling.Behavior;
using Mu.Sample.Account;
using ProtoBuf;

[ProtoContract]
public sealed record Opened
    : Fact<Account>
{
    public Opened(Owner owner)
    {
        Owner = owner;
    }

    [JsonConstructor]
    public Opened(Guid identity, DateTimeOffset proposed, Owner owner)
        : base(identity, proposed)
    {
        Owner = owner;
    }

    [ProtoMember(1, Name = "Owner")]
    public Owner Owner { get; }

    public static implicit operator Opened(Open open)
    {
        return new Opened(open.Owner);
    }
}