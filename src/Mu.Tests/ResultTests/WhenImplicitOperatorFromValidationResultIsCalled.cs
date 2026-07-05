namespace Mu.ResultTests;

using System.ComponentModel.DataAnnotations;
using Mu.Testing;

public sealed class WhenImplicitOperatorFromValidationResultIsCalled
{
    [Test]
    public async Task GivenFailureThenReturnsUnsuccessfulResult()
    {
        // Arrange
        ValidationResult failure = MuTestData.CreateFailure();

        // Act
        Result<string> result = failure;

        // Assert
        ValidationResult actual = await Assert.That(result.Failures).HasSingleItem();
        _ = await Assert.That(result.IsSuccessful).IsFalse();
        _ = await Assert.That(actual).IsSameReferenceAs(failure);
    }
}