namespace Muify.Service.NonMutationalStrategyTests;

using Muify;

public sealed class WhenApplyIsCalled
{
    [Test]
    public async Task GivenNothingThenTheNonMutationalAttributeShouldBeReturned()
    {
        // Arrange
        const string expected = """
            namespace Muify.Service
            {
                [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
                [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
                public sealed partial class NonMutationalAttribute
                    : global::System.Attribute
                {
                }
            }
            """;

        var strategy = new NonMutationalStrategy();

        // Act
        IEnumerable<File> result = strategy.Apply();

        // Assert
        File nonMutational = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(nonMutational.Content.ReplaceLineEndings()).IsEqualTo(expected.ReplaceLineEndings());
        _ = await Assert.That(nonMutational.Hint).IsEqualTo(NonMutationalStrategy.Hint);
    }
}