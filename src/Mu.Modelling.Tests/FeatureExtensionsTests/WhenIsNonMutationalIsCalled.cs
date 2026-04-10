namespace Mu.Modelling.FeatureExtensionsTests;

using MooVC.Syntax;

public sealed class WhenIsNonMutationalIsCalled
{
    private const string FeatureNameValue = "Feature";
    private const string ViewNameValue = "View";

    [Test]
    public async Task GivenBuilderThenFeatureIsNonMutational()
    {
        // Arrange
        Feature original = Feature.Undefined.Named(FeatureNameValue);

        // Act
        Feature result = original.IsNonMutational(nonMutational => nonMutational
            .FromWriteStore()
            .Using(view => view.Named(ViewNameValue)));

        // Assert
        _ = await Assert.That(result.Type).IsEqualTo(Feature.Kinds.NonMutational);
        _ = await Assert.That(result.NonMutational.View.Name).IsEqualTo(new Name(ViewNameValue));
        _ = await Assert.That(result.Mutational).IsEqualTo(Mutational.Undefined);
    }
}