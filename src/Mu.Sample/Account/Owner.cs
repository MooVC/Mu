namespace Mu.Sample.Account;

using ProtoBuf;

[ProtoContract]
public sealed record Owner([property: ProtoMember(1)] string Name)
{
    public static readonly Owner Unspecified = new(string.Empty);

    private Owner()
        : this(string.Empty)
    {
    }
}