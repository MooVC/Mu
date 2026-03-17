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
    public async Task GivenAnAssemblyThenTheAttributeIsGenerated()
    {
        foreach ((var assemblies, var language) in Frameworks.Enumerate(LanguageVersion.CSharp8))
        {
            // Arrange
            var test = new GeneratorTest<RaisesAttributeGenerator>(assemblies, language);

            Identity.IsExpectedIn(test.TestState);

            // Act
            Func<Task> act = () => test.RunAsync();

            // Assert
            await act.ShouldNotThrowAsync();
        }
    }
}