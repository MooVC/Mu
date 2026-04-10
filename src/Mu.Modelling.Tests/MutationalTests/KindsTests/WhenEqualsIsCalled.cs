namespace Mu.Modelling.MutationalTests.KindsTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public async Task GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Creational;
        Mutational.Kinds other = Mutational.Kinds.Transitional;

        // Act
        bool result = subject.Equals(other);

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenSameValueThenReturnsTrue()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Creational;
        Mutational.Kinds other = Mutational.Kinds.Creational;

        // Act
        bool result = subject.Equals(other);

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}