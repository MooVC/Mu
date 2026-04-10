namespace Mu.Modelling.NonMutationalTests.KindsTests;

public sealed class WhenEqualityOperatorKindsStringIsCalled
{
    private const string ReadStoreValue = "ReadStore";
    private const string WriteStoreValue = "WriteStore";

    [Test]
    public async Task GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kinds subject = NonMutational.Kinds.ReadStore;
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
        NonMutational.Kinds subject = NonMutational.Kinds.ReadStore;
        string value = ReadStoreValue;

        // Act
        bool result = subject == value;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}