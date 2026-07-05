namespace Mu.ResultTests;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Mu.Testing;

public sealed class WhenImplicitOperatorFromFailuresIsCalled
{
    [Test]
    public async Task GivenFailuresThenReturnsUnsuccessfulResult()
    {
        // Arrange
        ValidationResult first = MuTestData.CreateFailure();
        ValidationResult second = MuTestData.CreateFailure(MuTestData.AlternateFailureMessage);
        ImmutableArray<ValidationResult> failures = [first, second];

        // Act
        Result<string> result = failures;

        // Assert
        _ = await Assert.That(result.IsSuccessful).IsFalse();
        _ = await Assert.That(result.Failures).IsEquivalentTo(failures);
    }

    [Test]
    public async Task GivenEmptyFailuresThenThrowsArgumentException()
    {
        // Arrange
        ImmutableArray<ValidationResult> failures = [];

        // Act
        Exception? exception = Capture(() =>
        {
            Result<string> result = failures;

            _ = result;
        });

        // Assert
        _ = await Assert.That(exception).IsTypeOf<ArgumentException>();
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