namespace Mu.Architecture.Messaging;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

public sealed record Response<T>
    : Message
{
    internal Response(Context context, T value)
        : this(context, DateTimeOffset.UtcNow, value)
    {
    }

    [JsonConstructor]
    internal Response(Context context, DateTimeOffset preparedAt, T value)
        : base(context, preparedAt)
    {
        Value = value;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;

    public T Value { get; }

    public static implicit operator T(Response<T> response)
    {
        return response.Value;
    }
}