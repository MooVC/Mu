namespace Mu.Communications.Tracing.LedgerTests;

using Mu.Testing;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenCausationThenCreatesInitiatorLedger()
    {
        // Act
        var result = new Ledger(MuTestData.Identity);

        // Assert
        _ = await Assert.That(result.Causation).IsEqualTo(MuTestData.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(MuTestData.Identity);
        _ = await Assert.That(result.IsInitiator).IsTrue();
    }

    [Test]
    public async Task GivenCausationAndCorrelationThenCreatesContinuationLedger()
    {
        // Act
        Ledger result = MuTestData.CreateLedger();

        // Assert
        _ = await Assert.That(result.Causation).IsEqualTo(MuTestData.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(MuTestData.Correlation);
        _ = await Assert.That(result.IsInitiator).IsFalse();
    }
}