namespace Mu.Modelling.MutationalTests.KindsTests;

public sealed class WhenIsCreationalIsCalled
{
    [Test]
    public async Task GivenCreationalKindThenReturnsTrue()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Creational;

        // Act
        bool result = subject.IsCreational;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenTransitionalKindThenReturnsFalse()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Transitional;

        // Act
        bool result = subject.IsCreational;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }
}