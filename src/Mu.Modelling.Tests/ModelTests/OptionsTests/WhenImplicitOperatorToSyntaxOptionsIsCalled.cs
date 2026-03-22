namespace Mu.Modelling.ModelTests.OptionsTests;

using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using ModelOptions = Mu.Modelling.Options;
using SyntaxOptions = MooVC.Syntax.CSharp.Options;

public sealed class WhenImplicitOperatorToSyntaxOptionsIsCalled
{
    [Test]
    public async Task GivenOptionsThenSyntaxOptionsAreReturned()
    {
        // Arrange
        SyntaxOptions expected = SyntaxOptions.Default.WithNamespace(Qualifier.Options.Block);
        ModelOptions subject = new(ModelOptions.GithubOptions.Default, expected);

        // Act
        SyntaxOptions result = subject;

        // Assert
        _ = await Assert.That(result).IsEqualTo(expected);
    }
}