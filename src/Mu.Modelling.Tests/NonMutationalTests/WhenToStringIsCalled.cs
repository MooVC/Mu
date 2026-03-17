namespace Mu.Modelling.NonMutationalTests;

using MooVC.Syntax.Elements;

public sealed class WhenToStringIsCalled
{
    private const string ViewNameValue = "ViewName";

    [Test]
    public async Task GivenValuesThenContainsDetails()
    {
        // Arrange
        var view = new Name(ViewNameValue);
        NonMutational subject = ModellingTestData.CreateNonMutational(view: view);

        // Act
        string result = subject.ToString();

        // Assert
        await Assert.That(result.Contains(nameof(NonMutational))).IsTrue();
        await Assert.That(result.Contains(ViewNameValue)).IsTrue();
    }
}