namespace Mu.Communications.Messaging.IntentTests;

using Mu.Communications.Tracing;
using Mu.Testing;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenValuesThenPropertiesAreAssigned()
    {
        // Arrange
        Ledger ledger = MuTestData.CreateLedger();
        var useCase = new MuTestData.TestMutation();

        // Act
        var result = new Intent<MuTestData.TestMutation>(ledger, MuTestData.PreparedAt, useCase);

        // Assert
        _ = await Assert.That(result.Ledger).IsEqualTo(ledger);
        _ = await Assert.That(result.PreparedAt).IsEqualTo(MuTestData.PreparedAt);
        _ = await Assert.That(result.UseCase).IsSameReferenceAs(useCase);
    }

    [Test]
    public async Task GivenNullUseCaseThenThrowsArgumentNullException()
    {
        // Arrange
        MuTestData.TestMutation useCase = null!;

        // Act
        Exception? exception = Capture(() => _ = new Intent<MuTestData.TestMutation>(MuTestData.CreateLedger(), MuTestData.PreparedAt, useCase));

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