namespace Mu.Communications.Messaging.IntentTests;

using Mu.Testing;

public sealed class WhenYieldsIsCalled
{
    [Test]
    public async Task GivenResultThenReturnsOutcomeWithNextLedger()
    {
        // Arrange
        var useCase = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);
        var subject = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, useCase);
        Result<string> value = TestData.ResultValue;

        // Act
        Outcome<string> result = subject.Yields(value);

        // Assert
        _ = await Assert.That(result.Ledger.Causation).IsEqualTo(useCase.Identity);
        _ = await Assert.That(result.Ledger.Correlation).IsEqualTo(subject.Ledger.Correlation);
        _ = await Assert.That(result.Result).IsSameReferenceAs(value);
    }
}