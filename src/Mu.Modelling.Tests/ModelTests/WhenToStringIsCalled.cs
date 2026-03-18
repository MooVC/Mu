namespace Mu.Modelling.ModelTests;

using MooVC.Syntax.Elements;

public sealed class WhenToStringIsCalled
{
    private const string ModelNameValue = "ModelName";
    private const string CompanyNameValue = "CompanyName";

    [Test]
    public async Task GivenValuesThenContainsDetails()
    {
        // Arrange
        var name = new Name(ModelNameValue);
        var company = new Name(CompanyNameValue);
        Model subject = ModellingTestData.CreateModel(company: company, name: name);

        // Act
        string result = subject.ToString();

        // Assert
        _ = await Assert.That(result).Contains(nameof(Model));
        _ = await Assert.That(result).Contains(ModelNameValue);
        _ = await Assert.That(result).Contains(CompanyNameValue);
    }
}