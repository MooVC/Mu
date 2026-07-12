namespace Mu.Modelling.Services.ITransformExtensionsTests;

using Mu.Testing;

public sealed class WhenApplyAllIsCalled
{
    [Test]
    public async Task GivenSingleTransformAndFactsThenAppliesEachFact()
    {
        // Arrange
        TestData.TestAggregate aggregate = TestData.CreateAggregate();
        var first = new TestData.TestFact();
        var second = new TestData.TestFact(TestData.AlternateValue);
        var transform = new TestData.TestTransform();

        // Act
        TestData.TestAggregate result = transform.ApplyAll(aggregate, first, second);

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(aggregate.Value + first.Value + second.Value);
        _ = await Assert.That(transform.Facts).IsEquivalentTo(new[] { first, second });
    }

    [Test]
    public async Task GivenMultipleTransformsAndFactsThenAppliesEveryTransformToEveryFact()
    {
        // Arrange
        TestData.TestAggregate aggregate = TestData.CreateAggregate();
        var fact = new TestData.TestFact();
        var first = new TestData.TestTransform();
        var second = new TestData.TestTransform();
        ITransform<TestData.TestAggregate, TestData.TestFact>[] transforms = [first, second];

        // Act
        TestData.TestAggregate result = transforms.ApplyAll(aggregate, fact);

        // Assert
        _ = await Assert.That(result.Value).IsEqualTo(aggregate.Value + fact.Value + fact.Value);
        _ = await Assert.That(first.Facts).IsEquivalentTo(new[] { fact });
        _ = await Assert.That(second.Facts).IsEquivalentTo(new[] { fact });
    }
}