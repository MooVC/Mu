namespace Mu.Modelling.FeatureTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public void GivenSameValuesThenHashesMatch()
    {
        // Arrange
        Feature left = ModellingTestData.CreateFeature();
        Feature right = ModellingTestData.CreateFeature();

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        leftHash.ShouldBe(rightHash);
    }

    [Test]
    public void GivenDifferentValuesThenHashesDiffer()
    {
        // Arrange
        Feature left = ModellingTestData.CreateFeature();
        Feature right = ModellingTestData.CreateFeature(name: ModellingTestData.CreateAlternateName());

        // Act
        int leftHash = left.GetHashCode();
        int rightHash = right.GetHashCode();

        // Assert
        leftHash.ShouldNotBe(rightHash);
    }

    [Test]
    public void GivenSameInstanceThenHashIsStable()
    {
        // Arrange
        Feature subject = ModellingTestData.CreateFeature();

        // Act
        int firstHash = subject.GetHashCode();
        int secondHash = subject.GetHashCode();

        // Assert
        firstHash.ShouldBe(secondHash);
    }
}