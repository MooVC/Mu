namespace Mu.Modelling.MutationalTests.KindTests;

public sealed class WhenIsTransitionalIsCalled
{
    [Test]
    public async Task GivenTransitionalKindThenReturnsTrue()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Transitional;

        // Act
        bool result = subject.IsTransitional;

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenCreationalKindThenReturnsFalse()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Creational;

        // Act
        bool result = subject.IsTransitional;

        // Assert
        await Assert.That(result).IsFalse();
    }
}