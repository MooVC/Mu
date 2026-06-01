namespace Muify.Service.CreationalAttributeStrategyTests;

using Muify;

public sealed class WhenApplyIsCalled
{
    [Test]
    public async Task GivenNothingThenTheCreationalAttributeShouldBeReturned()
    {
        // Arrange
        const string expected = """
            namespace Muify.Service;

            [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
            public sealed partial class CreationalAttribute<TFact>
                : global::System.Attribute
                where TFact : global::Mu.Modelling.Behavior.Fact
            {
            }
            """;

        var strategy = new CreationalAttributeStrategy();

        // Act
        IEnumerable<File> result = strategy.Apply();

        // Assert
        File creational = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(creational.Content.ReplaceLineEndings()).IsEqualTo(expected.ReplaceLineEndings());
        _ = await Assert.That(creational.Hint).IsEqualTo(CreationalAttributeStrategy.Hint);
    }
}