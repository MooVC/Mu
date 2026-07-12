namespace Mu.Communications.Messaging.IntentTests;

using Mu.Communications.Tracing;
using Mu.Testing;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenValuesThenPropertiesAreAssigned()
    {
        // Arrange
        Ledger ledger = TestData.CreateLedger();
        var useCase = new TestData.TestMutation();

        // Act
        var result = new Intent<TestData.TestMutation>(ledger, TestData.PreparedAt, useCase);

        // Assert
        _ = await Assert.That(result.Ledger).IsEqualTo(ledger);
        _ = await Assert.That(result.PreparedAt).IsEqualTo(TestData.PreparedAt);
        _ = await Assert.That(result.UseCase).IsSameReferenceAs(useCase);
    }

    [Test]
    public async Task GivenNullUseCaseThenThrowsArgumentNullException()
    {
        // Arrange
        TestData.TestMutation useCase = null!;

        // Act
        Exception? exception = Capture(() => _ = new Intent<TestData.TestMutation>(TestData.CreateLedger(), TestData.PreparedAt, useCase));

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