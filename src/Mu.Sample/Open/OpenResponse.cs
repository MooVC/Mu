namespace Mu.Sample.Open;

using ProtoBuf;

[ProtoContract]
public sealed record OpenResponse
{
    [ProtoMember(2)]
    public string AccountId { get; init; } = string.Empty;

    [ProtoMember(3)]
    public string[] Failures { get; init; } = [];

    [ProtoMember(1)]
    public bool Successful { get; init; }
}