namespace Mu.Serialization.ProtobufTests;

using Mu.Communications.Messaging;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;
using Mu.Serialization;
using Mu.Testing;
using ProtoBuf.Meta;

public sealed class WhenDeepCloneIsCalled
{
    private const int ExpectedCount = 1;
    private const int PrimarySubtypeTag = 100;
    private const int SecondarySubtypeTag = 102;
    private const int TimestampOffsetMinutes = 345;

    [Test]
    public async Task GivenAggregateThenPreservesState()
    {
        // Arrange
        RuntimeTypeModel model = CreateModel();
        var offset = TimeSpan.FromMinutes(TimestampOffsetMinutes);
        var fact = new TestData.TestFact(TestData.Identity, TestData.ProposedAt.ToOffset(offset));
        Revision original = TestData.CreateRevision();
        var revision = new Revision(original.InitiatedAt.ToOffset(offset), original.Number);
        TestData.TestAggregate subject = TestData.CreateAggregateWithChanges(revision, fact);

        // Act
        TestData.TestAggregate result = model.DeepClone(subject);

        // Assert
        _ = await Assert.That(result.Propositions.Length).IsEqualTo(ExpectedCount);
        _ = await Assert.That(result.Revision).IsEqualTo(revision);
        _ = await Assert.That(result.Revision.InitiatedAt.Offset).IsEqualTo(offset);

        var resultFact = (TestData.TestFact)result.Propositions[0];
        _ = await Assert.That(resultFact.Identity).IsEqualTo(fact.Identity);
        _ = await Assert.That(resultFact.Proposed).IsEqualTo(fact.Proposed);
        _ = await Assert.That(resultFact.Proposed.Offset).IsEqualTo(offset);
        _ = await Assert.That(resultFact.Value).IsEqualTo(fact.Value);
    }

    [Test]
    public async Task GivenEventThenPreservesEnvelope()
    {
        // Arrange
        RuntimeTypeModel model = CreateModel();
        var offset = TimeSpan.FromMinutes(TimestampOffsetMinutes);
        var fact = new TestData.TestFact(TestData.Identity, TestData.ProposedAt.ToOffset(offset));

        Event<TestData.TestFact, Guid> subject = TestData.CreateEvent(
            fact: fact,
            committedAt: TestData.CommittedAt.ToOffset(offset),
            preparedAt: TestData.PreparedAt.ToOffset(offset));

        // Act
        Event<TestData.TestFact, Guid> result = model.DeepClone(subject);

        // Assert
        _ = await Assert.That(result.CommittedAt).IsEqualTo(subject.CommittedAt);
        _ = await Assert.That(result.CommittedAt.Offset).IsEqualTo(offset);
        _ = await Assert.That(result.Fact).IsNotNull();
        _ = await Assert.That(result.Ledger).IsEqualTo(subject.Ledger);
        _ = await Assert.That(((Event)result).Fact).IsSameReferenceAs(result.Fact);
        _ = await Assert.That(result.Fact.Identity).IsEqualTo(subject.Fact.Identity);
        _ = await Assert.That(result.Fact.Proposed).IsEqualTo(subject.Fact.Proposed);
        _ = await Assert.That(result.Fact.Proposed.Offset).IsEqualTo(offset);
        _ = await Assert.That(result.Origin).IsEqualTo(subject.Origin);
        _ = await Assert.That(result.PreparedAt).IsEqualTo(subject.PreparedAt);
        _ = await Assert.That(result.PreparedAt.Offset).IsEqualTo(offset);
    }

    [Test]
    public async Task GivenIntentThenPreservesEnvelope()
    {
        // Arrange
        RuntimeTypeModel model = CreateModel();
        var subject = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, new TestData.TestMutation());

        // Act
        Intent<TestData.TestMutation> result = model.DeepClone(subject);

        // Assert
        _ = await Assert.That(result.Ledger).IsEqualTo(subject.Ledger);
        _ = await Assert.That(result.PreparedAt).IsEqualTo(subject.PreparedAt);
        _ = await Assert.That(result.UseCase.Identity).IsEqualTo(subject.UseCase.Identity);
        _ = await Assert.That(result.UseCase.Proposed).IsEqualTo(subject.UseCase.Proposed);
    }

    [Test]
    public async Task GivenOutcomeThenPreservesEnvelope()
    {
        // Arrange
        RuntimeTypeModel model = CreateModel();
        var subject = new Outcome<string>(TestData.CreateLedger(), TestData.PreparedAt, TestData.ResultValue);

        // Act
        Outcome<string> result = model.DeepClone(subject);

        // Assert
        _ = await Assert.That(result.Ledger).IsEqualTo(subject.Ledger);
        _ = await Assert.That(result.PreparedAt).IsEqualTo(subject.PreparedAt);
        _ = await Assert.That(result.Result.IsSuccessful).IsTrue();
        _ = await Assert.That(result.Result.Value).IsEqualTo(TestData.ResultValue);
    }

    [Test]
    public async Task GivenFailedOutcomeThenPreservesFailures()
    {
        // Arrange
        RuntimeTypeModel model = CreateModel();
        var subject = new Outcome<string>(TestData.CreateLedger(), TestData.PreparedAt, TestData.CreateFailure());

        // Act
        Outcome<string> result = model.DeepClone(subject);

        // Assert
        _ = await Assert.That(result.Result.IsSuccessful).IsFalse();
        _ = await Assert.That(result.Result.Failures.Length).IsEqualTo(ExpectedCount);
        _ = await Assert.That(result.Result.Failures[0].ErrorMessage).IsEqualTo(TestData.FailureMessage);
    }

    [Test]
    public async Task GivenDateTimeOffsetThenPreservesInstantAndOffset()
    {
        // Arrange
        RuntimeTypeModel model = RuntimeTypeModel.Create().AddMu();
        var offset = TimeSpan.FromMinutes(TimestampOffsetMinutes);
        var subject = new DateTimeOffset(2024, 5, 6, 7, 8, 9, offset);

        // Act
        DateTimeOffset result = model.DeepClone(subject);

        // Assert
        _ = await Assert.That(result).IsEqualTo(subject);
        _ = await Assert.That(result.UtcTicks).IsEqualTo(subject.UtcTicks);
        _ = await Assert.That(result.Offset).IsEqualTo(subject.Offset);
    }

    [Test]
    public async Task GivenMessagingContractsThenSchemasAreValid()
    {
        // Arrange
        RuntimeTypeModel model = CreateModel();

        // Act
        string eventSchema = model.GetSchema(typeof(Event<TestData.TestFact, Guid>));
        string intentSchema = model.GetSchema(typeof(Intent<TestData.TestMutation>));
        string outcomeSchema = model.GetSchema(typeof(Outcome<string>));

        // Assert
        _ = await Assert.That(eventSchema).Contains(nameof(Event<TestData.TestFact, Guid>.CommittedAt));
        _ = await Assert.That(intentSchema).Contains(nameof(Intent<TestData.TestMutation>.UseCase));
        _ = await Assert.That(outcomeSchema).Contains(nameof(Outcome<string>.Result));
    }

    private static RuntimeTypeModel CreateModel()
    {
        RuntimeTypeModel model = RuntimeTypeModel.Create().AddMu();

        _ = model.Add(typeof(Aggregate), true).AddSubType(PrimarySubtypeTag, typeof(TestData.TestAggregate));
        _ = model.Add(typeof(Fact), true).AddSubType(PrimarySubtypeTag, typeof(Fact<TestData.TestAggregate>));
        _ = model[typeof(Fact<TestData.TestAggregate>)].AddSubType(PrimarySubtypeTag, typeof(TestData.TestFact));
        _ = model.Add(typeof(Mutational), true).AddSubType(SecondarySubtypeTag, typeof(TestData.TestMutation));

        return model;
    }
}