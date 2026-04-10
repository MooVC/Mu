namespace Mu.Modelling.MutationalTests.KindsTests;

public sealed class WhenEqualityOperatorKindsStringIsCalled
{
    private const string CreationalValue = "Creational";
    private const string TransitionalValue = "Transitional";

    [Test]
    public async Task GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Creational;
        string value = TransitionalValue;

        // Act
        bool result = subject == value;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenMatchingValueThenReturnsTrue()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Creational;
        string value = CreationalValue;

        // Act
        bool result = subject == value;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}