namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenEqualityOperatorKindStringIsCalled
{
    private const string ReadStoreValue = "ReadStore";
    private const string WriteStoreValue = "WriteStore";

    [Test]
    public void GivenMatchingValueThenReturnsTrue()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;
        string value = ReadStoreValue;

        // Act
        bool result = subject == value;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;
        string value = WriteStoreValue;

        // Act
        bool result = subject == value;

        // Assert
        result.ShouldBeFalse();
    }
}