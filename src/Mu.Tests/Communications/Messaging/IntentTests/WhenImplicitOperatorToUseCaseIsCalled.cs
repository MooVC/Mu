namespace Mu.Communications.Messaging.IntentTests;

using Mu.Testing;

public sealed class WhenImplicitOperatorToUseCaseIsCalled
{
    [Test]
    public async Task GivenIntentThenReturnsUseCase()
    {
        // Arrange
        var useCase = new MuTestData.TestMutation();
        var subject = new Intent<MuTestData.TestMutation>(MuTestData.CreateLedger(), MuTestData.PreparedAt, useCase);

        // Act
        MuTestData.TestMutation result = subject;

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(useCase);
    }

    [Test]
    public async Task GivenNullIntentThenThrowsArgumentNullException()
    {
        // Arrange
        Intent<MuTestData.TestMutation> subject = null!;

        // Act
        Exception? exception = Capture(() =>
        {
            MuTestData.TestMutation result = subject;

            _ = result;
        });

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