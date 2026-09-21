namespace Mu.Communications.Tracing.ScopeTests;

using Mu.Testing;

public sealed class WhenConstructorForUseCaseIsCalled
{
    [Test]
    public async Task GivenUseCaseWithoutAmbientScopeThenCreatesInitiatorLedger()
    {
        // Arrange
        var useCase = new TestData.TestMutation(TestData.Identity, TestData.ProposedAt);

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
        using var outer = new Scope(TestData.CreateLedger());
        var useCase = new TestData.TestMutation(TestData.AlternateIdentity, TestData.ProposedAt);

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
        const string expectedMessage = "The use case used to create the tracing ledger must be provided.";
        TestData.TestMutation useCase = null!;

        // Act
        Exception? exception = Capture(() => _ = new Scope(useCase));

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentNullException>();
        var argumentException = (ArgumentNullException)exception!;
        _ = await Assert.That(argumentException.Message).StartsWith(expectedMessage);
        _ = await Assert.That(argumentException.ParamName).IsEqualTo(nameof(useCase));
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