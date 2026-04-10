namespace Mu.Modelling.NonMutationalTests.KindsTests;

public sealed class WhenIsWriteStoreIsCalled
{
    [Test]
    public async Task GivenReadStoreKindThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kinds subject = NonMutational.Kinds.ReadStore;

        // Act
        bool result = subject.IsWriteStore;

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenWriteStoreKindThenReturnsTrue()
    {
        // Arrange
        NonMutational.Kinds subject = NonMutational.Kinds.WriteStore;

        // Act
        bool result = subject.IsWriteStore;

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}