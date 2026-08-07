namespace Mu.Sample.Open;

using Mu.Modelling.Behavior;
using Mu.Sample.Account;
using ProtoBuf;

[ProtoContract(Name = "Open", SkipConstructor = true)]
public sealed record Open([property: ProtoMember(1, Name = "Owner")] Owner Owner)
    : Creational<Account>
{
    [ProtoContract(Name = "Result", SkipConstructor = true)]
    public sealed record Result([property: ProtoMember(1, Name = "Id")] Guid Id);
}