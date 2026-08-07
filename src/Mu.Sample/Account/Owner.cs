namespace Mu.Sample.Account;

using ProtoBuf;

[ProtoContract(SkipConstructor = true)]
public sealed record Owner([property: ProtoMember(1, Name = "Name")] string Name)
{
    public static readonly Owner Unspecified = new(string.Empty);
}