namespace Mu.Communications.Messaging;

using System;
using System.Text.Json.Serialization;
using Mu.Communications.Tracing;
using Mu.Modelling.Behavior;

/// <summary>
/// Wraps a use case as a synchronous intent message.
/// </summary>
/// <typeparam name="TUseCase">The use case type being expressed.</typeparam>
public sealed record Intent<TUseCase>
    : Message
    where TUseCase : UseCase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Intent{TUseCase}"/> record.
    /// </summary>
    internal Intent(Ledger ledger, TUseCase useCase)
        : this(ledger, DateTimeOffset.UtcNow, useCase)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Intent{TUseCase}"/> record.
    /// </summary>
    [JsonConstructor]
    internal Intent(Ledger ledger, DateTimeOffset preparedAt, TUseCase useCase)
        : base(ledger, preparedAt)
    {
        ArgumentNullException.ThrowIfNull(useCase);

        UseCase = useCase;
    }

    /// <summary>
    /// Gets the use case carried by the intent.
    /// </summary>
    public TUseCase UseCase { get; }

    /// <summary>
    /// Converts an intent to its wrapped use case.
    /// </summary>
    public static implicit operator TUseCase(Intent<TUseCase> request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.UseCase;
    }

    /// <summary>
    /// Creates an outcome message from a use case execution result.
    /// </summary>
    public Outcome<TResult> Yields<TResult>(Result<TResult> result)
        where TResult : notnull
    {
        Ledger ledger = Ledger.Next(UseCase.Identity);

        return new(ledger, result);
    }
}