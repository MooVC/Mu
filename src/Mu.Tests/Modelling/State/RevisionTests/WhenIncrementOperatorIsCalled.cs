namespace Mu.Modelling.State.RevisionTests;

using Mu.Testing;

public sealed class WhenIncrementOperatorIsCalled
{
    [Test]
    public async Task GivenRevisionThenNumberIsIncrementedAndInitiatedAtIsUpdated()
    {
        // Arrange
        Revision subject = MuTestData.CreateRevision();

        // Act
        Revision result = subject;
        result++;

        // Assert
        _ = await Assert.That(result.InitiatedAt).IsNotEqualTo(subject.InitiatedAt);
        _ = await Assert.That(result.Number).IsEqualTo(subject.Number + 1);
    }
}