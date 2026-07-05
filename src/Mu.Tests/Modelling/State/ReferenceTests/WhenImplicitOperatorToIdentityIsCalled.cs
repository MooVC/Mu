namespace Mu.Modelling.State.ReferenceTests;

using Mu.Testing;

public sealed class WhenImplicitOperatorToIdentityIsCalled
{
    [Test]
    public async Task GivenReferenceThenReturnsIdentity()
    {
        // Arrange
        Reference<Guid> subject = MuTestData.CreateReference();

        // Act
        Guid result = subject;

        // Assert
        _ = await Assert.That(result).IsEqualTo(MuTestData.Identity);
    }
}