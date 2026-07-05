namespace Mu.Modelling.Services.ITransformExtensionsTests;

using Mu.Testing;

public sealed class WhenApplyAllIsCalled
{
    [Test]
    public async Task GivenSingleTransformAndFactsThenAppliesEachFact()
    {
        // Arrange
        MuTestData.TestAggregate aggregate = MuTestData.CreateAggregate();
        var first = new MuTestData.TestFact();
        var second = new MuTestData.TestFact(MuTestData.AlternateValue);
        var transform = new MuTestData.TestTransform();

        // Act
        MuTestData.TestAggregate result = transform.ApplyAll(aggregate, first, second);

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(aggregate.Value + first.Value + second.Value);
        _ = await Assert.That(transform.Facts).IsEquivalentTo(new[] { first, second });
    }

    [Test]
    public async Task GivenMultipleTransformsAndFactsThenAppliesEveryTransformToEveryFact()
    {
        // Arrange
        MuTestData.TestAggregate aggregate = MuTestData.CreateAggregate();
        var fact = new MuTestData.TestFact();
        var first = new MuTestData.TestTransform();
        var second = new MuTestData.TestTransform();
        ITransform<MuTestData.TestAggregate, MuTestData.TestFact>[] transforms = [first, second];

        // Act
        MuTestData.TestAggregate result = transforms.ApplyAll(aggregate, fact);

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(aggregate.Value + fact.Value + fact.Value);
        _ = await Assert.That(first.Facts).IsEquivalentTo(new[] { fact });
        _ = await Assert.That(second.Facts).IsEquivalentTo(new[] { fact });
    }
}