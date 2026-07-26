namespace Mu.Sample.Open;

using Mu.Modelling.Behavior;
using Mu.Sample.Account;
using ProtoBuf;

[ProtoContract]
public sealed record Open([property: ProtoMember(1)] Owner Owner)
    : Creational<Account>
{
    private Open()
        : this(Owner.Unspecified)
    {
    }
}