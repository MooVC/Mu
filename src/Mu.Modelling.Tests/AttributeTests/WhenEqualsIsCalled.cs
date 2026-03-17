namespace Mu.Modelling.AttributeTests;

using ModellingAttribute = Mu.Modelling.Attribute;

public sealed class WhenEqualsIsCalled
{
    [Test]
    public void GivenEqualValuesThenReturnsTrue()
    {
        // Arrange
        ModellingAttribute left = ModellingTestData.CreateAttribute();
        ModellingAttribute right = ModellingTestData.CreateAttribute();

        // Act
        bool result = left.Equals(right);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void GivenNullThenReturnsFalse()
    {
        // Arrange
        ModellingAttribute subject = ModellingTestData.CreateAttribute();

        // Act
        bool result = subject.Equals(null);

        // Assert
        result.ShouldBeFalse();
    }
}