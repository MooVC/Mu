namespace Mu.Modelling.NonMutationalTests.KindsTests;

public sealed class WhenToStringIsCalled
{
    [Test]
    public async Task GivenReadStoreValueThenReturnsName()
    {
        // Arrange
        NonMutational.Kinds subject = NonMutational.Kinds.ReadStore;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(NonMutational.Kinds.ReadStore));
    }

    [Test]
    public async Task GivenWriteStoreValueThenReturnsName()
    {
        // Arrange
        NonMutational.Kinds subject = NonMutational.Kinds.WriteStore;

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).IsEqualTo(nameof(NonMutational.Kinds.WriteStore));
    }
}