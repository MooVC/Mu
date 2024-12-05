namespace Mu.Architecture.Messaging;

using System;

public abstract record Message
{
    private protected Message(Context context)
        : this(context, DateTimeOffset.UtcNow)
    {
    }

    private protected Message(Context context, DateTimeOffset preparedAt)
    {
        Context = context;
        PreparedAt = preparedAt;
    }

    public Context Context { get; }

    public DateTimeOffset PreparedAt { get; }
}