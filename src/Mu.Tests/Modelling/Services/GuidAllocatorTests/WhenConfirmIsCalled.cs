namespace Mu.Modelling.Services.GuidAllocatorTests;

using Mu.Testing;

public sealed class WhenConfirmIsCalled
{
    [Test]
    public async Task GivenIdentityThenCompletes()
    {
        // Arrange
        var subject = new GuidAllocator();

        // Act
        ValueTask result = subject.Confirm(MuTestData.Identity, CancellationToken.None);

        // Assert
        _ = await Assert.That(result.IsCompletedSuccessfully).IsTrue();
        await result;
    }
}