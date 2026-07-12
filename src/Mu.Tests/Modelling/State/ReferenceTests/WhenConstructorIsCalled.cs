namespace Mu.Modelling.State.ReferenceTests;

using Mu.Testing;

public sealed class WhenConstructorIsCalled
{
    [Test]
    public async Task GivenValuesThenPropertiesAreAssigned()
    {
        // Act
        Reference<Guid> result = TestData.CreateReference();

        // Assert
        _ = await Assert.That(result.Identity).IsEqualTo(TestData.Identity);
        _ = await Assert.That(result.IsUnspecified).IsFalse();
        _ = await Assert.That(result.Revision).IsEqualTo(3ul);
    }

    [Test]
    public async Task GivenZeroRevisionThenIsUnspecified()
    {
        // Act
        Reference<Guid> result = TestData.CreateReference(revision: 0);

        // Assert
        _ = await Assert.That(result.IsUnspecified).IsTrue();
    }
}