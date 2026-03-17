namespace Mu.Modelling.NonMutationalTests;

public sealed class WhenGetHashCodeIsCalled
{
    [Test]
    public void GivenSameValuesThenHashesMatch()
    {
        // Arrange
        NonMutational left = ModellingTestData.CreateNonMutational();
        NonMutational right = ModellingTestData.CreateNonMutational();

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
        NonMutational left = ModellingTestData.CreateNonMutational();
        NonMutational right = ModellingTestData.CreateNonMutational(view: ModellingTestData.CreateAlternateName());

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
        NonMutational subject = ModellingTestData.CreateNonMutational();

        // Act
        int firstHash = subject.GetHashCode();
        int secondHash = subject.GetHashCode();

        // Assert
        firstHash.ShouldBe(secondHash);
    }
}