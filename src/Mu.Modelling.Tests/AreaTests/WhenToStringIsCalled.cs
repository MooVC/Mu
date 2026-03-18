namespace Mu.Modelling.AreaTests;

using MooVC.Syntax.Elements;

public sealed class WhenToStringIsCalled
{
    private const string AreaNameValue = "AreaName";

    [Test]
    public async Task GivenValuesThenContainsDetails()
    {
        // Arrange
        var name = new Name(AreaNameValue);
        Area subject = ModellingTestData.CreateArea(name: name);

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).Contains(nameof(Area));
        _ = await Assert.That(result).Contains(AreaNameValue);
    }
}