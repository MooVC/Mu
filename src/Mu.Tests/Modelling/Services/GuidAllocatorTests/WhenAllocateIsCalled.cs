namespace Mu.Modelling.Services.GuidAllocatorTests;

using Mu.Testing;

public sealed class WhenAllocateIsCalled
{
    [Test]
    public async Task GivenUseCaseThenReturnsVersionSevenGuidForProposedTime()
    {
        // Arrange
        var useCase = new MuTestData.TestMutation(MuTestData.Identity, MuTestData.ProposedAt);
        var subject = new GuidAllocator();

        // Act
        Guid result = await subject.Allocate(useCase, CancellationToken.None);

        // Assert
        _ = await Assert.That(result.ToString("N")[12]).IsEqualTo('7');
    }
}