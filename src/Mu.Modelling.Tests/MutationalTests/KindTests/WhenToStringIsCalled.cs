namespace Mu.Modelling.MutationalTests.KindTests;

public sealed class WhenToStringIsCalled
{
    [Test]
    public async Task GivenCreationalValueThenReturnsName()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Creational;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(Mutational.Kind.Creational));
    }

    [Test]
    public async Task GivenTransitionalValueThenReturnsName()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Transitional;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(Mutational.Kind.Transitional));
    }
}