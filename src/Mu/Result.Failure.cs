namespace Mu;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Ardalis.GuardClauses;
using ProtoBuf;
using static Mu.Failure_Resources;

public static partial class Result
{
    [ProtoContract]
    internal sealed record Failure
    {
        [ProtoMember(1, Name = nameof(ErrorMessage))]
        public string? ErrorMessage { get; init; }

        [ProtoMember(2, Name = nameof(MemberNames))]
        public ImmutableArray<string> MemberNames { get; init; } = [];

        public static implicit operator Failure(ValidationResult failure)
        {
            _ = Guard.Against.Null(failure, message: ImplicitFailureRequired);

            return new Failure
            {
                ErrorMessage = failure.ErrorMessage,
                MemberNames = [.. failure.MemberNames],
            };
        }

        public static implicit operator ValidationResult(Failure failure)
        {
            _ = Guard.Against.Null(failure, message: ImplicitFailureRequired);

            return new ValidationResult(failure.ErrorMessage, failure.MemberNames);
        }
    }
}