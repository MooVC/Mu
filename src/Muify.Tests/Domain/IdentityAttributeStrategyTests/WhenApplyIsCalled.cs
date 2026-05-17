namespace Muify.Domain.IdentityAttributeStrategyTests;

using Muify;

public sealed class WhenApplyIsCalled
{
    [Test]
    public async Task GivenNothingThenTheIdentityAttributeShouldBeReturned()
    {
        // Arrange
        const string expected = """
            namespace Muify.Domain;

            [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
            [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
            public sealed partial class IdentityAttribute
                : global::System.Attribute
            {
            }
            """;

        var strategy = new IdentityAttributeStrategy();

        // Act
        IEnumerable<File> result = strategy.Apply();

        // Assert
        File identity = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(identity.Content).IsEqualTo(expected);
        _ = await Assert.That(identity.Hint).IsEqualTo(IdentityAttributeStrategy.Hint);
    }
}