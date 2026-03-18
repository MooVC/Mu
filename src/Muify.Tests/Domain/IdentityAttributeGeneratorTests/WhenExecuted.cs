namespace Muify.Domain.IdentityAttributeGeneratorTests;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

public sealed class WhenExecuted
{
    public const string Content = """
        namespace Muify.Domain
        {
            [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
            internal sealed partial class IdentityAttribute
                : global::System.Attribute
            {
            }
        }
        """;

    public static readonly Generated Identity = new(
        Content,
        typeof(IdentityAttributeGenerator),
        IdentityAttributeGenerator.Hint);

    [Test]
    [Skip("There appears to be an issue that prevents the generator from completing before the result is returned.")]
    public async Task GivenAnAssemblyThenTheAttributeIsGenerated()
    {
        foreach (Theory theory in Frameworks.Enumerate(LanguageVersion.CSharp8))
        {
            // Arrange
            var test = new GeneratorTest<IdentityAttributeGenerator>(theory.Assemblies, theory.Language);

            Identity.IsExpectedIn(test.TestState);

            // Act
            Func<Task> act = () => test.RunAsync();

            // Assert
            await act();
        }
    }
}