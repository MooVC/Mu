namespace Mu.Modelling.MutationalTests.KindsTests;

public sealed class WhenIsTransitionalIsCalled
{
    [Test]
    public async Task GivenCreationalKindThenReturnsFalse()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Creational;

        // Act
        bool result = subject.IsTransitional;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenTransitionalKindThenReturnsTrue()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Transitional;

        // Act
        bool result = subject.IsTransitional;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}