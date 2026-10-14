namespace Mu.Serialization.RuntimeTypeModelExtensionsTests;

using Mu.Modelling.Behavior;
using Mu.Modelling.State;
using Mu.Testing;
using ProtoBuf.Meta;

public sealed class WhenAddMuIsCalled
{
    private const int OffsetMinutes = 90;

    private const ulong TargetRevision = 7;

    private const long TimestampPrecision = 1234;

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenACreationalMessageThenItsInheritedMetadataAndPayloadRoundtrip(bool autoCompile)
    {
        // Arrange
        RuntimeTypeModel model = RuntimeTypeModel.Create();
        model.AutoCompile = autoCompile;
        _ = model.AddMu();
        MetaType contract = model.Add(typeof(TestData.TestCreational), applyDefaultBehaviour: false);
        contract.UseConstructor = false;
        _ = contract.Add(1, nameof(Causal.Identity));
        _ = contract.Add(2, nameof(Causal.Proposed));
        _ = contract.Add(4, nameof(TestData.TestCreational.Value));
        DateTimeOffset proposed = CreateTimestamp();
        var expected = new TestData.TestCreational(TestData.Identity, proposed, TestData.AlternateValue);
        using var buffer = new MemoryStream();

        // Act
        model.Serialize(buffer, expected);
        buffer.Position = 0;
        var result = (TestData.TestCreational)model.Deserialize(buffer, value: null, type: typeof(TestData.TestCreational));

        // Assert
        _ = await Assert.That(result.Identity).IsEqualTo(expected.Identity);
        _ = await Assert.That(result.Proposed.Ticks).IsEqualTo(proposed.Ticks);
        _ = await Assert.That(result.Proposed.Offset).IsEqualTo(proposed.Offset);
        _ = await Assert.That(result.Value).IsEqualTo(expected.Value);
        _ = await Assert.That(result.Model).IsEqualTo(expected.Model);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenATransitionalMessageThenItsInheritedMetadataAndTargetRoundtrip(bool autoCompile)
    {
        // Arrange
        RuntimeTypeModel model = RuntimeTypeModel.Create();
        model.AutoCompile = autoCompile;
        _ = model.AddMu();
        MetaType contract = model.Add(typeof(TestData.TestTransitional), applyDefaultBehaviour: false);
        contract.UseConstructor = false;
        _ = contract.Add(1, nameof(Causal.Identity));
        _ = contract.Add(2, nameof(Causal.Proposed));
        _ = contract.Add(4, nameof(TestData.TestTransitional.Value));
        _ = contract.Add(5, nameof(TestData.TestTransitional.Target));
        DateTimeOffset proposed = CreateTimestamp();
        var expected = new TestData.TestTransitional(TestData.Identity, proposed, TestData.CreateReference(TestData.AlternateIdentity, TargetRevision), TestData.AlternateValue);
        using var buffer = new MemoryStream();

        // Act
        model.Serialize(buffer, expected);
        buffer.Position = 0;
        var result = (TestData.TestTransitional)model.Deserialize(buffer, value: null, type: typeof(TestData.TestTransitional));

        // Assert
        _ = await Assert.That(result.Identity).IsEqualTo(expected.Identity);
        _ = await Assert.That(result.Proposed.Ticks).IsEqualTo(proposed.Ticks);
        _ = await Assert.That(result.Proposed.Offset).IsEqualTo(proposed.Offset);
        _ = await Assert.That(result.Target.Identity).IsEqualTo(expected.Target.Identity);
        _ = await Assert.That(result.Target.Revision).IsEqualTo(expected.Target.Revision);
        _ = await Assert.That(result.Value).IsEqualTo(expected.Value);
        _ = await Assert.That(result.Model).IsEqualTo(expected.Model);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenASuccessfulRevisionResultThenItsNumberAndTimestampRoundtrip(bool autoCompile)
    {
        // Arrange
        RuntimeTypeModel model = RuntimeTypeModel.Create();
        model.AutoCompile = autoCompile;
        _ = model.AddMu();
        DateTimeOffset initiatedAt = CreateTimestamp();
        Result<Revision> expected = new Revision(initiatedAt, TargetRevision);
        using var buffer = new MemoryStream();

        // Act
        model.Serialize(buffer, expected);
        buffer.Position = 0;
        var result = (Result<Revision>)model.Deserialize(buffer, value: null, type: typeof(Result<Revision>));

        // Assert
        _ = await Assert.That(result.IsSuccessful).IsTrue();
        _ = await Assert.That(result.Value.Number).IsEqualTo(expected.Value.Number);
        _ = await Assert.That(result.Value.InitiatedAt.Ticks).IsEqualTo(initiatedAt.Ticks);
        _ = await Assert.That(result.Value.InitiatedAt.Offset).IsEqualTo(initiatedAt.Offset);
    }

    private static DateTimeOffset CreateTimestamp()
    {
        return TestData.ProposedAt
            .ToOffset(TimeSpan.FromMinutes(OffsetMinutes))
            .AddTicks(TimestampPrecision);
    }
}