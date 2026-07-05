namespace Mu.Communications.Tracing.ScopeTests;

using Mu.Testing;

public sealed class WhenConstructorForUseCaseIsCalled
{
    [Test]
    public async Task GivenUseCaseWithoutAmbientScopeThenCreatesInitiatorLedger()
    {
        // Arrange
        var useCase = new MuTestData.TestMutation(MuTestData.Identity, MuTestData.ProposedAt);

        // Act
        using var scope = new Scope(useCase);

        // Assert
        _ = await Assert.That(scope.Ledger.Causation).IsEqualTo(useCase.Identity);
        _ = await Assert.That(scope.Ledger.Correlation).IsEqualTo(useCase.Identity);
    }

    [Test]
    public async Task GivenUseCaseWithAmbientScopeThenCreatesContinuationLedger()
    {
        // Arrange
        using var outer = new Scope(MuTestData.CreateLedger());
        var useCase = new MuTestData.TestMutation(MuTestData.AlternateIdentity, MuTestData.ProposedAt);

        // Act
        using var scope = new Scope(useCase);

        // Assert
        _ = await Assert.That(scope.Ledger.Causation).IsEqualTo(useCase.Identity);
        _ = await Assert.That(scope.Ledger.Correlation).IsEqualTo(outer.Ledger.Correlation);
    }

    [Test]
    public async Task GivenNullUseCaseThenThrowsArgumentNullException()
    {
        // Arrange
        MuTestData.TestMutation useCase = null!;

        // Act
        Exception? exception = Capture(() => _ = new Scope(useCase));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
    }

    private static Exception? Capture(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        return default;
    }
}