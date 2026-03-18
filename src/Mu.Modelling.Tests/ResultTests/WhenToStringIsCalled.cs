namespace Mu.Modelling.ResultTests;

using MooVC.Syntax.Elements;

public sealed class WhenToStringIsCalled
{
    private const string ResultNameValue = "ResultName";

    [Test]
    public async Task GivenValuesThenContainsDetails()
    {
        // Arrange
        var name = new Name(ResultNameValue);
        Result subject = ModellingTestData.CreateResult(name: name);

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).Contains(nameof(Result));
        _ = await Assert.That(result).Contains(ResultNameValue);
    }
}