namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenIsWriteStoreIsCalled
{
    [Test]
    public async Task GivenWriteStoreKindThenReturnsTrue()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.WriteStore;

        // Act
        bool result = subject.IsWriteStore;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenReadStoreKindThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;

        // Act
        bool result = subject.IsWriteStore;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }
}