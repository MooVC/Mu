namespace Mu.Modelling.State.RevisionTests;

using Mu.Testing;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenDefaultConstructorThenReturnsInitialRevision()
    {
        // Act
        var result = new Revision();

        // Assert
        _ = await Assert.That(result.InitiatedAt).IsEqualTo(DateTimeOffset.MinValue);
        _ = await Assert.That(result.Number).IsEqualTo(0ul);
    }

    [Test]
    public async Task GivenValuesThenPropertiesAreAssigned()
    {
        // Act
        Revision result = MuTestData.CreateRevision();

        // Assert
        _ = await Assert.That(result.InitiatedAt).IsEqualTo(MuTestData.PreparedAt);
        _ = await Assert.That(result.Number).IsEqualTo(3ul);
    }
}