namespace Mu.Modelling.UnitTests;

using MooVC.Syntax;

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
        _ = await Assert.That(result).Contains(nameof(Unit));
        _ = await Assert.That(result).Contains(UnitNameValue);
    }
}