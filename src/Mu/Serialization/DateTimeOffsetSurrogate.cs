namespace Mu.Serialization;

using ProtoBuf;

[ProtoContract]
internal readonly record struct DateTimeOffsetSurrogate
{
    public DateTimeOffsetSurrogate(long ticks, int offsetMinutes)
    {
        Ticks = ticks;
        OffsetMinutes = offsetMinutes;
    }

    [ProtoMember(1, Name = nameof(Ticks))]
    public long Ticks { get; private init; }

    [ProtoMember(2, Name = nameof(OffsetMinutes))]
    public int OffsetMinutes { get; private init; }

    public static implicit operator DateTimeOffset(DateTimeOffsetSurrogate surrogate)
    {
        return new DateTimeOffset(surrogate.Ticks, TimeSpan.FromMinutes(surrogate.OffsetMinutes));
    }

    public static implicit operator DateTimeOffsetSurrogate(DateTimeOffset value)
    {
        return new DateTimeOffsetSurrogate(value.Ticks, (int)value.Offset.TotalMinutes);
    }
}