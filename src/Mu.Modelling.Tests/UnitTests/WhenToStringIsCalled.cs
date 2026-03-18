namespace Mu.Modelling.UnitTests;

using MooVC.Syntax.Elements;

public sealed class WhenToStringIsCalled
{
    private const string UnitNameValue = "UnitName";

    [Test]
    public async Task GivenValuesThenContainsDetails()
    {
        // Arrange
        var name = new Name(UnitNameValue);
        Unit subject = ModellingTestData.CreateUnit(name: name);

        // Act
        string result = subject.ToString();

        // Assert
        await Assert.That(result).Contains(nameof(Unit));
        await Assert.That(result).Contains(UnitNameValue);
    }
}