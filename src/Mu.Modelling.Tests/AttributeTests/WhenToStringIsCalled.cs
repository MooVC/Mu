namespace Mu.Modelling.AttributeTests;

using MooVC.Syntax;
using ModellingAttribute = Mu.Modelling.Attribute;

public sealed class WhenToStringIsCalled
{
    private const string AttributeNameValue = "AttributeName";

    [Test]
    public async Task GivenValuesThenContainsDetails()
    {
        // Arrange
        var name = new Name(AttributeNameValue);
        ModellingAttribute subject = ModellingTestData.CreateAttribute(name: name);

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).Contains(nameof(Attribute));
        _ = await Assert.That(result).Contains(AttributeNameValue);
    }
}