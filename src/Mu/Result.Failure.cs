namespace Mu;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using ProtoBuf;

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
            ArgumentNullException.ThrowIfNull(failure);

            return new Failure
            {
                ErrorMessage = failure.ErrorMessage,
                MemberNames = [.. failure.MemberNames],
            };
        }

        public static implicit operator ValidationResult(Failure failure)
        {
            ArgumentNullException.ThrowIfNull(failure);

            return new ValidationResult(failure.ErrorMessage, failure.MemberNames);
        }
    }
}