namespace Mu.Modelling.MutationalTests.KindTests;

public sealed class WhenEqualityOperatorKindStringIsCalled
{
    private const string CreationalValue = "Creational";
    private const string TransitionalValue = "Transitional";
    [Test]
    public async Task GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        Mutational.Kind subject = Mutational.Kind.Creational;
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
        Mutational.Kind subject = Mutational.Kind.Creational;
        string value = CreationalValue;

        // Act
        bool result = subject == value;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}