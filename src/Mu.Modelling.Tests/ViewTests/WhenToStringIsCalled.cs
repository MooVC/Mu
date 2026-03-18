namespace Mu.Modelling.ViewTests;

using MooVC.Syntax.Elements;

public sealed class WhenToStringIsCalled
{
    private const string ViewNameValue = "ViewName";

    [Test]
    public async Task GivenValuesThenContainsDetails()
    {
        // Arrange
        var name = new Name(ViewNameValue);
        View subject = ModellingTestData.CreateView(name: name);

        // Act
        string result = subject.ToString();

        // Assert
        await Assert.That(result).Contains(nameof(View));
        await Assert.That(result).Contains(ViewNameValue);
    }
}