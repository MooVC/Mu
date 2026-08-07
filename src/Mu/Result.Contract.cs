namespace Mu;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using ProtoBuf;

public static partial class Result
{
    [ProtoContract]
    internal sealed record Contract<T>
        where T : notnull
    {
        [ProtoMember(1, Name = nameof(IsSuccessful))]
        public bool IsSuccessful { get; init; }

        [ProtoMember(2, Name = nameof(Failures))]
        public ImmutableArray<Failure> Failures { get; init; } = [];

        [ProtoMember(3, Name = nameof(Value))]
        public T? Value { get; init; }

        public static implicit operator Contract<T>(Result<T>? result)
        {
            if (result is null)
            {
                return new Contract<T>();
            }

            return result.IsSuccessful
                ? new Contract<T> { IsSuccessful = true, Value = result.Value }
                : new Contract<T> { Failures = [.. result.Failures.Select(static failure => (Failure)failure)] };
        }

        public static implicit operator Result<T>(Contract<T> contract)
        {
            ArgumentNullException.ThrowIfNull(contract);

            if (contract.IsSuccessful)
            {
                return contract.Value!;
            }

            ImmutableArray<ValidationResult> failures = [.. contract.Failures.Select(static failure => (ValidationResult)failure)];

            return failures;
        }
    }
}