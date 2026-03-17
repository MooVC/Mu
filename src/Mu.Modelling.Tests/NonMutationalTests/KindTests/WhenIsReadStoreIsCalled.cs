namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenIsReadStoreIsCalled
{
    [Test]
    public async Task GivenReadStoreKindThenReturnsTrue()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;

        // Act
        bool result = subject.IsReadStore;

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenWriteStoreKindThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.WriteStore;

        // Act
        bool result = subject.IsReadStore;

        // Assert
        await Assert.That(result).IsFalse();
    }
}