namespace Mu.Modelling.Services.GuidAllocatorTests;

using Mu.Testing;

public sealed class WhenSurrenderIsCalled
{
    [Test]
    public async Task GivenIdentityThenCompletes()
    {
        // Arrange
        var subject = new GuidAllocator();

        // Act
        ValueTask result = subject.Surrender(MuTestData.Identity, CancellationToken.None);

        // Assert
        _ = await Assert.That(result.IsCompletedSuccessfully).IsTrue();
        await result;
    }
}