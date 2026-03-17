namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenToStringIsCalled
{
    [Test]
    public void GivenReadStoreValueThenReturnsName()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;

        // Act
        string result = subject.ToString();

        // Assert
        result.ShouldBe(nameof(NonMutational.Kind.ReadStore));
    }

    [Test]
    public void GivenWriteStoreValueThenReturnsName()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.WriteStore;

        // Act
        string result = subject.ToString();

        // Assert
        result.ShouldBe(nameof(NonMutational.Kind.WriteStore));
    }
}