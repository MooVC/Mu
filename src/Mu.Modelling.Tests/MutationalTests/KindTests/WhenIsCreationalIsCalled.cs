namespace Mu.Modelling.MutationalTests.KindTests;

public sealed class WhenIsCreationalIsCalled
{
    [Test]
    public void GivenCreationalKindThenReturnsTrue()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Creational;

        // Act
        bool result = subject.IsCreational;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenTransitionalKindThenReturnsFalse()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Transitional;

        // Act
        bool result = subject.IsCreational;

        // Assert
        result.ShouldBeFalse();
    }
}