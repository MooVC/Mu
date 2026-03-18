namespace Mu.Modelling.NonMutationalTests.KindTests;

public sealed class WhenToStringIsCalled
{
    [Test]
    public async Task GivenReadStoreValueThenReturnsName()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.ReadStore;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(NonMutational.Kind.ReadStore));
    }

    [Test]
    public async Task GivenWriteStoreValueThenReturnsName()
    {
        // Arrange
        NonMutational.Kind subject = NonMutational.Kind.WriteStore;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(NonMutational.Kind.WriteStore));
    }
}