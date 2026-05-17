namespace Muify.Domain.UnitAttributeStrategyTests;

using Muify;

public sealed class WhenApplyIsCalled
{
    [Test]
    public async Task GivenNothingThenTheUnitAttributeShouldBeReturned()
    {
        // Arrange
        const string expected = """
            namespace Muify.Domain;

            [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
            public sealed partial class UnitAttribute<TIdentity>
                : global::System.Attribute
                where TIdentity : struct
            {
            }
            """;

        var strategy = new UnitAttributeStrategy();

        // Act
        IEnumerable<File> result = strategy.Apply();

        // Assert
        File unit = await Assert.That(result).HasSingleItem();
        _ = await Assert.That(unit.Content.ReplaceLineEndings()).IsEqualTo(expected.ReplaceLineEndings());
        _ = await Assert.That(unit.Hint).IsEqualTo(UnitAttributeStrategy.Hint);
    }
}