namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenIsReadStoreIsCalled
{
    [Test]
    public void GivenReadStoreKindThenReturnsTrue()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;

        // Act
        bool result = subject.IsReadStore;

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenWriteStoreKindThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.WriteStore;

        // Act
        bool result = subject.IsReadStore;

        // Assert
        result.ShouldBeFalse();
    }
}