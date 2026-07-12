namespace Mu.Communications.Tracing.LedgerTests;

using Mu.Testing;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenCausationThenCreatesInitiatorLedger()
    {
        // Act
        var result = new Ledger(TestData.Identity);

        // Assert
        _ = await Assert.That(result.Causation).IsEqualTo(TestData.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(TestData.Identity);
        _ = await Assert.That(result.IsInitiator).IsTrue();
    }

    [Test]
    public async Task GivenCausationAndCorrelationThenCreatesContinuationLedger()
    {
        // Act
        Ledger result = TestData.CreateLedger();

        // Assert
        _ = await Assert.That(result.Causation).IsEqualTo(TestData.Identity);
        _ = await Assert.That(result.Correlation).IsEqualTo(TestData.Correlation);
        _ = await Assert.That(result.IsInitiator).IsFalse();
    }
}