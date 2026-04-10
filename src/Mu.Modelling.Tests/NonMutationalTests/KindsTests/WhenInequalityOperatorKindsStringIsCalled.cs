namespace Mu.Modelling.NonMutationalTests.KindsTests;

public sealed class WhenInequalityOperatorKindsStringIsCalled
{
    private const string ReadStoreValue = "ReadStore";
    private const string WriteStoreValue = "WriteStore";

    [Test]
    public async Task GivenDifferentValueThenReturnsTrue()
    {
        // Arrange
        NonMutational.Kinds subject = NonMutational.Kinds.ReadStore;
        string value = WriteStoreValue;

        // Act
        bool result = subject != value;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenMatchingValueThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kinds subject = NonMutational.Kinds.ReadStore;
        string value = ReadStoreValue;

        // Act
        bool result = subject != value;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }
}