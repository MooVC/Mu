namespace Mu.Communications.Tracing.LedgerTests;

using Mu.Testing;

public sealed class WhenNextIsCalled
{
    [Test]
    public async Task GivenCausationThenReturnsLedgerWithSameCorrelation()
    {
        // Arrange
        Ledger subject = MuTestData.CreateLedger();

        // Act
        Ledger result = subject.Next(MuTestData.AlternateIdentity);

        // Assert
        _ = await Assert.That(result.Causation).IsEqualTo(MuTestData.AlternateIdentity);
        _ = await Assert.That(result.Correlation).IsEqualTo(subject.Correlation);
    }
}