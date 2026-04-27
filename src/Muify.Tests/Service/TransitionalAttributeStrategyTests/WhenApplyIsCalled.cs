namespace Muify.Service.TransitionalAttributeStrategyTests;

using Muify;

public sealed class WhenApplyIsCalled
{
    [Test]
    public async Task GivenNothingThenTheTransitionalAttributeShouldBeReturned()
    {
        // Arrange
        const string expected = """
            namespace Muify.Service
            {
                [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
                [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
                public sealed partial class TransitionalAttribute
                    : global::System.Attribute
                {
                    public string Fact { get; set; }
                }
            }
            """;

        var strategy = new TransitionalAttributeStrategy();

        // Act
        IEnumerable<File> result = strategy.Apply();

        // Assert
        File transitional = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(transitional.Content.ReplaceLineEndings()).IsEqualTo(expected.ReplaceLineEndings());
        _ = await Assert.That(transitional.Hint).IsEqualTo(TransitionalAttributeStrategy.Hint);
    }
}