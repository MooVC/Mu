namespace Muify.Domain.IdentityAttributeGeneratorTests;

using Microsoft.CodeAnalysis.CSharp;

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
    public async Task GivenAnAssemblyThenTheAttributeIsGenerated()
    {
        foreach ((var assemblies, var language) in Frameworks.Enumerate(LanguageVersion.CSharp8))
        {
            // Arrange
            var test = new GeneratorTest<IdentityAttributeGenerator>(assemblies, language);

            Identity.IsExpectedIn(test.TestState);

            // Act
            Func<Task> act = () => test.RunAsync();

            // Assert
            await act.ShouldNotThrowAsync();
        }
    }
}