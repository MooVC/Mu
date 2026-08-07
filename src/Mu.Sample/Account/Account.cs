namespace Mu.Sample.Account;

using Mu.Modelling.State;
using ProtoBuf;

[ProtoContract]
public sealed record Account
    : Aggregate
{
    [ProtoMember(3, Name = nameof(Owner))]
    public Owner Owner { get; init; } = Owner.Unspecified;
}