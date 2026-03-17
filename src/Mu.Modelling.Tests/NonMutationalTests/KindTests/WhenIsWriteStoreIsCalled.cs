namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenIsWriteStoreIsCalled
{
    [Test]
    public void GivenWriteStoreKindThenReturnsTrue()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.WriteStore;

        // Act
        bool result = subject.IsWriteStore;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenReadStoreKindThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;

        // Act
        bool result = subject.IsWriteStore;

        // Assert
        result.ShouldBeFalse();
    }
}