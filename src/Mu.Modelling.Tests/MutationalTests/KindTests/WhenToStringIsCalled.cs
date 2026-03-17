namespace Mu.Modelling.MutationalTests.KindTests;

public sealed class WhenToStringIsCalled
{
    [Test]
    public void GivenCreationalValueThenReturnsName()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Creational;

        // Act
        string result = subject.ToString();

        // Assert
        result.ShouldBe(nameof(Mutational.Kind.Creational));
    }

    [Test]
    public void GivenTransitionalValueThenReturnsName()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Transitional;

        // Act
        string result = subject.ToString();

        // Assert
        result.ShouldBe(nameof(Mutational.Kind.Transitional));
    }
}