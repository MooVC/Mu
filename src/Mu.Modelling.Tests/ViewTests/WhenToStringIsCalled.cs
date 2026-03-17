namespace Mu.Modelling.ViewTests;

using MooVC.Syntax.Elements;

public sealed class WhenToStringIsCalled
{
    private const string ViewNameValue = "ViewName";

    [Test]
    public void GivenValuesThenContainsDetails()
    {
        // Arrange
        var name = new Name(ViewNameValue);
        View subject = ModellingTestData.CreateView(name: name);

        // Act
        string result = subject.ToString();

        // Assert
        result.ShouldContain(nameof(View));
        result.ShouldContain(ViewNameValue);
    }
}