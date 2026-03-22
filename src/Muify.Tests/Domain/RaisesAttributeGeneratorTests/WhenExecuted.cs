namespace Muify.Domain.RaisesAttributeGeneratorTests;

using Microsoft.CodeAnalysis.CSharp;

public sealed class WhenExecuted
{
    public const string Content = """
        namespace Muify.Domain
        {
            [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
            internal sealed partial class RaisesAttribute
                : global::System.Attribute
            {
                public string Name { get; set; }
            }
        }
        """;

    public static readonly Generated Identity = new(
        Content,
        typeof(RaisesAttributeGenerator),
        RaisesAttributeGenerator.Hint);

    [Test]
    [Skip("There appears to be an issue that prevents the generator from completing before the result is returned.")]
    public async Task GivenAnAssemblyThenTheAttributeIsGenerated()
    {
        foreach (Theory theory in Frameworks.Enumerate(LanguageVersion.CSharp8))
        {
            // Arrange
            var test = new GeneratorTest<RaisesAttributeGenerator>(theory.Assemblies, theory.Language);

            Identity.IsExpectedIn(test.TestState);

            // Act & Assert
            await test.RunAsync();
        }
    }
}