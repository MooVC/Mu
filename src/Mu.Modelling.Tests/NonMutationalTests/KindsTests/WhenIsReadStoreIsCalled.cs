namespace Mu.Modelling.NonMutationalTests.KindsTests;

public sealed class WhenIsReadStoreIsCalled
{
    [Test]
    public async Task GivenReadStoreKindThenReturnsTrue()
    {
        // Arrange
        NonMutational.Kinds subject = NonMutational.Kinds.ReadStore;

        // Act
        bool result = subject.IsReadStore;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task GivenWriteStoreKindThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kinds subject = NonMutational.Kinds.WriteStore;

        // Act
        bool result = subject.IsReadStore;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }
}