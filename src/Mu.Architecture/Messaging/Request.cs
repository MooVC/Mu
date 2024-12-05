namespace Mu.Architecture.Messaging;

using System;
using System.Text.Json.Serialization;
using Mu.Architecture.Modelling;

public sealed record Request<T>
    : Message
    where T : UseCase
{
    public Request(Context context, T useCase)
        : this(context, DateTimeOffset.UtcNow, useCase)
    {
    }

    [JsonConstructor]
    internal Request(Context context, DateTimeOffset preparedAt, T useCase)
        : base(context, preparedAt)
    {
        ArgumentNullException.ThrowIfNull(UseCase);

        UseCase = useCase;
    }

    public T UseCase { get; }

    public static implicit operator T(Request<T> request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.UseCase;
    }
}