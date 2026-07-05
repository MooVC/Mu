namespace Mu.Modelling.Integrity.SpecificationTests;

using System.ComponentModel.DataAnnotations;
using Mu.Testing;

public sealed class WhenEnforceIsCalled
{
    [Test]
    public async Task GivenIntentThenReturnsFailuresFromImplementation()
    {
        // Arrange
        ValidationResult failure = MuTestData.CreateFailure();
        var subject = new MuTestData.TestSpecification<TestQuery>(failure);
        var intent = new TestQuery();

        // Act
        ValidationResult[] result = await ToArray(subject.Enforce(intent, CancellationToken.None));

        // Assert
        _ = await Assert.That(result).IsEquivalentTo(new[] { failure });
        _ = await Assert.That(subject.Intents).IsEquivalentTo(new[] { intent });
    }

    [Test]
    public async Task GivenNullIntentThenThrowsArgumentNullException()
    {
        // Arrange
        var subject = new MuTestData.TestSpecification<TestQuery>();
        TestQuery intent = null!;

        // Act
        Exception? exception = Capture(() => _ = subject.Enforce(intent, CancellationToken.None));

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

    private static async Task<ValidationResult[]> ToArray(IAsyncEnumerable<ValidationResult> values)
    {
        var results = new List<ValidationResult>();

        await foreach (ValidationResult value in values)
        {
            results.Add(value);
        }

        return [.. results];
    }
}