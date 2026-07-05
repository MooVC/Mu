namespace Mu.Testing;

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Mu.Communications.Messaging;
using Mu.Communications.PubSub;
using Mu.Communications.Tracing;
using Mu.Modelling;
using Mu.Modelling.Behavior;
using Mu.Modelling.Integrity;
using Mu.Modelling.Services;
using Mu.Modelling.State;

public static class MuTestData
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

    public sealed record AlternateFact
        : Fact<TestAggregate>;

    public sealed record TestAggregate(int Value)
        : Aggregate
    {
        public TestAggregate()
            : this(DefaultValue)
        {
        }
    }

    public sealed class TestAllocator
        : IAllocator<Guid>
    {
        public IList<Guid> Confirmed { get; } = [];

        public Guid Identity { get; init; } = MuTestData.Identity;

        public IList<UseCase> Requests { get; } = [];

        public IList<Guid> Surrendered { get; } = [];

        public ValueTask<Guid> Allocate<TUseCase>(TUseCase useCase, CancellationToken cancellationToken)
            where TUseCase : UseCase
        {
            Requests.Add(useCase);

            return ValueTask.FromResult(Identity);
        }

        public ValueTask Confirm(Guid identity, CancellationToken cancellationToken)
        {
            Confirmed.Add(identity);

            return ValueTask.CompletedTask;
        }

        public ValueTask Surrender(Guid identity, CancellationToken cancellationToken)
        {
            Surrendered.Add(identity);

            return ValueTask.CompletedTask;
        }
    }

    public sealed record TestCreational
        : Creational<TestAggregate>
    {
        public TestCreational(int value = FactValue)
        {
            Value = value;
        }

        public TestCreational(Guid identity, DateTimeOffset proposed, int value = FactValue)
            : base(identity, proposed)
        {
            Value = value;
        }

        public int Value { get; init; }
    }

    public sealed record TestFact
        : Fact<TestAggregate>,
          IConvertFrom<TestFact, TestCreational>,
          IConvertFrom<TestFact, TestMutation>,
          IConvertFrom<TestFact, TestTransitional>
    {
        public TestFact(int value = FactValue)
        {
            Value = value;
        }

        public TestFact(Guid identity, DateTimeOffset proposed, int value = FactValue)
            : base(identity, proposed)
        {
            Value = value;
        }

        public int Value { get; init; }

        public static implicit operator TestFact(TestCreational subject)
        {
            return new TestFact(subject.Value);
        }

        public static implicit operator TestFact(TestMutation subject)
        {
            return new TestFact(subject.Value);
        }

        public static implicit operator TestFact(TestTransitional subject)
        {
            return new TestFact(subject.Value);
        }
    }

    public sealed class TestInvariant<TMutation>
        : IInvariant<TestAggregate, TMutation>
        where TMutation : Mutational
    {
        private readonly ImmutableArray<ValidationResult> _failures;

        public TestInvariant(params ValidationResult[] failures)
        {
            _failures = [.. failures];
        }

        public IList<TestAggregate> Aggregates { get; } = [];

        public IList<TMutation> Mutations { get; } = [];

        public async IAsyncEnumerable<ValidationResult> Enforce(TestAggregate aggregate, TMutation mutation, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Aggregates.Add(aggregate);
            Mutations.Add(mutation);

            foreach (ValidationResult failure in _failures)
            {
                await Task.CompletedTask;

                yield return failure;
            }
        }
    }

    public sealed record TestMutation
        : Mutational
    {
        public TestMutation(int value = FactValue)
        {
            Value = value;
        }

        public TestMutation(Guid identity, DateTimeOffset proposed, int value = FactValue)
            : base(identity, proposed)
        {
            Value = value;
        }

        public override Representation Model => typeof(TestAggregate);

        public int Value { get; init; }
    }

    public sealed class TestPolicy
        : IPolicy<TestFact, Guid>
    {
        private readonly ConcurrentBag<Event<TestFact, Guid>> _events = [];

        public IReadOnlyCollection<Event<TestFact, Guid>> Events => _events.ToArray();

        public Task Apply(Event<TestFact, Guid> @event, CancellationToken cancellationToken)
        {
            _events.Add(@event);

            return Task.CompletedTask;
        }
    }

    public sealed class TestSpecification<TIntent>
        : Specification<TIntent>
        where TIntent : NonMutational
    {
        private readonly ImmutableArray<ValidationResult> _failures;

        public TestSpecification(params ValidationResult[] failures)
        {
            _failures = [.. failures];
        }

        public IList<TIntent> Intents { get; } = [];

        protected override async IAsyncEnumerable<ValidationResult> PerformEnforce(TIntent intent, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Intents.Add(intent);

            foreach (ValidationResult failure in _failures)
            {
                await Task.CompletedTask;

                yield return failure;
            }
        }
    }

    public sealed class TestSubscriber
        : ISubscriber
    {
        public event ISubscriber.EventReceivedHandler? Received;

        public int StartCount { get; private set; }

        public int StopCount { get; private set; }

        public Task Raise(Event @event, CancellationToken cancellationToken)
        {
            return Received?.Invoke(this, @event, cancellationToken) ?? Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartCount++;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            StopCount++;

            return Task.CompletedTask;
        }
    }

    public sealed class TestTransform
        : ITransform<TestAggregate, Fact>,
          ITransform<TestAggregate, TestFact>
    {
        public IList<TestFact> Facts { get; } = [];

        public TestAggregate Apply(TestAggregate aggregate, TestFact fact)
        {
            Facts.Add(fact);

            return aggregate with
            {
                Value = aggregate.Value + fact.Value,
            };
        }

        TestAggregate ITransform<TestAggregate, Fact>.Apply(TestAggregate aggregate, Fact fact)
        {
            return Apply(aggregate, (TestFact)fact);
        }
    }

    public sealed record TestTransitional
        : Transitional<TestAggregate, Guid>
    {
        public TestTransitional(Reference<Guid> target, int value = FactValue)
            : base(target)
        {
            Value = value;
        }

        public TestTransitional(Guid identity, DateTimeOffset proposed, Reference<Guid> target, int value = FactValue)
            : base(identity, proposed, target)
        {
            Value = value;
        }

        public int Value { get; init; }
    }
}