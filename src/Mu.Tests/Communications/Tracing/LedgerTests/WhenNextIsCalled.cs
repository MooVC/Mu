namespace Mu.Communications.Tracing.LedgerTests;

using Mu.Testing;

public sealed class WhenNextIsCalled
{
    [Test]
    public async Task GivenCausationThenReturnsLedgerWithSameCorrelation()
    {
        // Arrange
        Ledger subject = TestData.CreateLedger();

        // Act
        Ledger result = subject.Next(TestData.AlternateIdentity);

        // Assert
        _ = await Assert.That(result.Causation).IsEqualTo(TestData.AlternateIdentity);
        _ = await Assert.That(result.Correlation).IsEqualTo(subject.Correlation);
    }
}