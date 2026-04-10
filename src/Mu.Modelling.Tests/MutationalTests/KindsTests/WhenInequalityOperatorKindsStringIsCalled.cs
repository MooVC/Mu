namespace Mu.Modelling.MutationalTests.KindsTests;

public sealed class WhenInequalityOperatorKindsStringIsCalled
{
    private const string CreationalValue = "Creational";
    private const string TransitionalValue = "Transitional";

    [Test]
    public async Task GivenDifferentValueThenReturnsTrue()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Creational;
        string value = TransitionalValue;

        // Act
        bool result = subject != value;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenMatchingValueThenReturnsFalse()
    {
        // Arrange
        Mutational.Kinds subject = Mutational.Kinds.Creational;
        string value = CreationalValue;

        // Act
        bool result = subject != value;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }
}