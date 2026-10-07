namespace Muify.PostInitializationGeneratorTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public sealed class WhenInitializeIsCalled
{
    [Test]
    public async Task GivenACompilationThenGeneratedAttributeHintsIncludeTheirNamespaces()
    {
        // Arrange
        const string assemblyName = "Muify.Testing.Generated.Attributes";
        string[] expectedHints =
        [
            "Muify.Domain.IdentityAttribute.g.cs",
            "Muify.Domain.UnitAttribute.g.cs",
            "Muify.Service.CreationalAttribute.g.cs",
            "Muify.Service.NonMutationalAttribute.g.cs",
            "Muify.Service.TransitionalAttribute.g.cs",
        ];

        var compilation = CSharpCompilation.Create(assemblyName);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new PostInitializationGenerator());

        // Act
        GeneratorRunResult result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

        // Assert
        _ = await Assert.That(result.Diagnostics).IsEmpty();
        string[] hints = result.GeneratedSources.Select(definition => definition.HintName).ToArray();

        foreach (string expectedHint in expectedHints)
        {
            _ = await Assert.That(hints).Contains(expectedHint);
        }
    }
}