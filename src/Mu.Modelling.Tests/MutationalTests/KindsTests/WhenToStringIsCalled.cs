namespace Mu.Modelling.MutationalTests.KindsTests;

public sealed class WhenToStringIsCalled
{
    [Test]
    public async Task GivenCreationalValueThenReturnsName()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Creational;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(Mutational.Kinds.Creational));
    }

    [Test]
    public async Task GivenTransitionalValueThenReturnsName()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Transitional;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(Mutational.Kinds.Transitional));
    }
}