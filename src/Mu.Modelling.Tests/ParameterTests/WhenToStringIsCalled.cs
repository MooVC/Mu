namespace Mu.Modelling.ParameterTests;

using MooVC.Syntax.Elements;

public sealed class WhenToStringIsCalled
{
    private const string ParameterNameValue = "ParameterName";

    [Test]
    public async Task GivenValuesThenContainsDetails()
    {
        // Arrange
        var name = new Name(ParameterNameValue);
        Parameter subject = ModellingTestData.CreateParameter(name: name);

        // Act
        string result = subject.ToString();

        // Assert
        await Assert.That(result.Contains(nameof(Parameter))).IsTrue();
        await Assert.That(result.Contains(ParameterNameValue)).IsTrue();
    }
}