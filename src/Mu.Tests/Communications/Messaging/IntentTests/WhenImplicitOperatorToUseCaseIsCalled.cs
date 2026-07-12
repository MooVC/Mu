namespace Mu.Communications.Messaging.IntentTests;

using Mu.Testing;

public sealed class WhenImplicitOperatorToUseCaseIsCalled
{
    [Test]
    public async Task GivenIntentThenReturnsUseCase()
    {
        // Arrange
        var useCase = new TestData.TestMutation();
        var subject = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, useCase);

        // Act
        TestData.TestMutation result = subject;

        // Assert
        _ = await Assert.That(result).IsSameReferenceAs(useCase);
    }

    [Test]
    public async Task GivenNullIntentThenThrowsArgumentNullException()
    {
        // Arrange
        Intent<TestData.TestMutation> subject = null!;

        // Act
        Exception? exception = Capture(() =>
        {
            TestData.TestMutation result = subject;

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