namespace Mu.Testing;

using System.ComponentModel.DataAnnotations;
using Mu.Communications.Messaging;
using Mu.Communications.Tracing;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;

public static partial class TestData
{
    public const int AlternateValue = 17;
    public const int DefaultValue = 11;
    public const int FactValue = 5;
    public const string AlternateFailureMessage = "Alternate failure.";
    public const string FailureMessage = "Failure.";
    public const string ResultValue = "Result";

    public static readonly Guid AlternateIdentity = Guid.Parse("22222222-2222-7222-8222-222222222222");
    public static readonly DateTimeOffset CommittedAt = new(2024, 4, 5, 6, 7, 8, TimeSpan.Zero);
    public static readonly Guid Correlation = Guid.Parse("33333333-3333-7333-8333-333333333333");
    public static readonly Guid Identity = Guid.Parse("11111111-1111-7111-8111-111111111111");
    public static readonly DateTimeOffset PreparedAt = new(2024, 3, 4, 5, 6, 7, TimeSpan.Zero);
    public static readonly DateTimeOffset ProposedAt = new(2024, 2, 3, 4, 5, 6, TimeSpan.Zero);

    public static TestAggregate CreateAggregate(int value = DefaultValue)
    {
        return new TestAggregate(value);
    }

    public static TestAggregate CreateAggregateWithChanges(Revision revision, params Fact[] propositions)
    {
        return new TestAggregate(DefaultValue)
        {
            Propositions = [.. propositions],
            Revision = revision,
        };
    }

    public static Event<TestFact, Guid> CreateEvent(
        TestFact? fact = null,
        Reference<Guid> origin = default,
        Ledger ledger = default,
        DateTimeOffset committedAt = default,
        DateTimeOffset preparedAt = default)
    {
        return new Event<TestFact, Guid>(
            committedAt == default ? CommittedAt : committedAt,
            ledger == default ? CreateLedger() : ledger,
            fact ?? new TestFact(),
            origin == default ? CreateReference() : origin,
            preparedAt == default ? PreparedAt : preparedAt);
    }

    public static ValidationResult CreateFailure(string message = FailureMessage)
    {
        return new ValidationResult(message);
    }

    public static Ledger CreateLedger(Guid causation = default, Guid correlation = default)
    {
        return new Ledger(
            causation == Guid.Empty ? Identity : causation,
            correlation == Guid.Empty ? Correlation : correlation);
    }

    public static Reference<Guid> CreateReference(Guid identity = default, ulong revision = 3)
    {
        return new Reference<Guid>(identity == Guid.Empty ? Identity : identity, revision);
    }

    public static Revision CreateRevision(ulong number = 3)
    {
        return new Revision(PreparedAt, number);
    }
}