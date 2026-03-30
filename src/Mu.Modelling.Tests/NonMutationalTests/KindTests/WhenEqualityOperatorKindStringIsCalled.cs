namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenEqualityOperatorKindStringIsCalled
{
    private const string ReadStoreValue = "ReadStore";
    private const string WriteStoreValue = "WriteStore";

    [Test]
    public async Task GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;
        string value = WriteStoreValue;

        // Act
        bool result = subject == value;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenMatchingValueThenReturnsTrue()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;
        string value = ReadStoreValue;

        // Act
        bool result = subject == value;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}