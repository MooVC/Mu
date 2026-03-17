namespace Mu.Modelling.FeatureExtensionsTests;

using MooVC.Syntax.Elements;

public sealed class WhenIsMutationalIsCalled
{
    private const string FeatureNameValue = "Feature";
    private const string RegisteredFactValue = "Registered";

    [Test]
    public async Task GivenBuilderThenFeatureIsMutational()
    {
        // Arrange
        Feature original = Feature.Undefined.Named(new Name(FeatureNameValue));

        // Act
        Feature result = original.IsMutational(mutational => mutational
            .IsCreational()
            .Raises(new Name(RegisteredFactValue)));

        // Assert
        await Assert.That(result.Type).IsEqualTo(Feature.Kind.Mutational);
        await Assert.That(result.Mutational.Fact).IsEqualTo(new Name(RegisteredFactValue));
        await Assert.That(result.NonMutational).IsEqualTo(NonMutational.Undefined);
    }
}