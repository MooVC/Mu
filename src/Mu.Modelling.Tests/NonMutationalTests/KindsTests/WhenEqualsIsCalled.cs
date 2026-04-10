namespace Mu.Modelling.NonMutationalTests.KindsTests;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public async Task GivenDifferentValueThenReturnsFalse()
    {
        // Arrange
        NonMutational.Kinds subject = NonMutational.Kinds.ReadStore;
        NonMutational.Kinds other = NonMutational.Kinds.WriteStore;

        // Act
        bool result = subject.Equals(other);

        // Assert
        _ = await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task GivenSameValueThenReturnsTrue()
    {
        // Arrange
        NonMutational.Kinds subject = NonMutational.Kinds.ReadStore;
        NonMutational.Kinds other = NonMutational.Kinds.ReadStore;

        // Act
        bool result = subject.Equals(other);

        // Assert
        _ = await Assert.That(result).IsTrue();
    }
}