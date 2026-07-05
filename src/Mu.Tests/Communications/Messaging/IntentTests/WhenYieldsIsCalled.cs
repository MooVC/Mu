namespace Mu.Communications.Messaging.IntentTests;

using Mu.Testing;

public sealed class WhenYieldsIsCalled
{
    [Test]
    public async Task GivenResultThenReturnsOutcomeWithNextLedger()
    {
        // Arrange
        var useCase = new MuTestData.TestMutation(MuTestData.AlternateIdentity, MuTestData.ProposedAt);
        var subject = new Intent<MuTestData.TestMutation>(MuTestData.CreateLedger(), MuTestData.PreparedAt, useCase);
        Result<string> value = MuTestData.ResultValue;

        // Act
        Outcome<string> result = subject.Yields(value);

        // Assert
        _ = await Assert.That(result.Ledger.Causation).IsEqualTo(useCase.Identity);
        _ = await Assert.That(result.Ledger.Correlation).IsEqualTo(subject.Ledger.Correlation);
        _ = await Assert.That(result.Result).IsSameReferenceAs(value);
    }
}