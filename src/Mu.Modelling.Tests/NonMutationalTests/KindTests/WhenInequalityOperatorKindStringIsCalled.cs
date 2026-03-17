namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenInequalityOperatorKindStringIsCalled
{
    private const string ReadStoreValue = "ReadStore";
    private const string WriteStoreValue = "WriteStore";

    [Test]
    public async Task GivenDifferentValueThenReturnsTrue()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;
        string value = WriteStoreValue;

        // Act
        bool result = subject != value;

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenMatchingValueThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;
        string value = ReadStoreValue;

        // Act
        bool result = subject != value;

        // Assert
        await Assert.That(result).IsFalse();
    }
}