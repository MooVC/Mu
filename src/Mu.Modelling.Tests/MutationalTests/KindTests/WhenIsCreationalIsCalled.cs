namespace Mu.Modelling.MutationalTests.KindTests;

public sealed class WhenIsCreationalIsCalled
{
    [Test]
    public async Task GivenCreationalKindThenReturnsTrue()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Creational;

        // Act
        bool result = subject.IsCreational;

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenTransitionalKindThenReturnsFalse()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Transitional;

        // Act
        bool result = subject.IsCreational;

        // Assert
        await Assert.That(result).IsFalse();
    }
}