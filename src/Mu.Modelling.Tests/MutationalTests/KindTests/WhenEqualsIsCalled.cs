namespace Mu.Modelling.MutationalTests.KindTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public async Task GivenSameValueThenReturnsTrue()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Creational;
        Mutational.Kind other = Mutational.Kind.Creational;

        // Act
        bool result = subject.Equals(other);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Creational;
        Mutational.Kind other = Mutational.Kind.Transitional;

        // Act
        bool result = subject.Equals(other);

        // Assert
        await Assert.That(result).IsFalse();
    }
}