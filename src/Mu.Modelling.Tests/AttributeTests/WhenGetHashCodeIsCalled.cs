namespace Mu.Modelling.AttributeTests;

using ModellingAttribute = Mu.Modelling.Attribute;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public async Task GivenSameValuesThenHashesMatch()
    {
        // Arrange
        ModellingAttribute left = ModellingTestData.CreateAttribute();
        ModellingAttribute right = ModellingTestData.CreateAttribute();

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        _ = await Assert.That(leftHash).IsEqualTo(rightHash);
    }

    [Test]
    public async Task GivenDifferentValuesThenHashesDiffer()
    {
        // Arrange
        ModellingAttribute left = ModellingTestData.CreateAttribute();
        ModellingAttribute right = ModellingTestData.CreateAttribute(name: ModellingTestData.CreateAlternateName());

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        _ = await Assert.That(leftHash).IsNotEqualTo(rightHash);
    }

    [Test]
    public async Task GivenSameInstanceThenHashIsStable()
    {
        // Arrange
        ModellingAttribute subject = ModellingTestData.CreateAttribute();

        // Act
        int firstHash = subject.GetHashCode();
        int secondHash = subject.GetHashCode();

        // Assert
        _ = await Assert.That(firstHash).IsEqualTo(secondHash);
    }
}