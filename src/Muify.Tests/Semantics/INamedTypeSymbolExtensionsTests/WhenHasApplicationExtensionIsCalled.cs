namespace Muify.Semantics.INamedTypeSymbolExtensionsTests;

using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public sealed class WhenHasApplicationExtensionIsCalled
{
    private const string ApplicationType = "Microsoft.AspNetCore.Builder.WebApplication";

    private const string AssemblyName = "MooVC.Testing.Mechanics.Car.Open";

    [Test]
    [Arguments("public static WebApplication BuildOpen(this WebApplication app) { return app; }", true)]
    [Arguments("public static WebApplication BuildOpen(WebApplication app, int optional = default) { return app; }", true)]
    [Arguments("extension(WebApplication app) { public WebApplication BuildOpen() { return app; } }", true)]
    [Arguments("extension(WebApplication app) { public WebApplication BuildOpen(int optional = default) { return app; } }", true)]
    [Arguments("public static WebApplication BuildOpen(this WebApplicationBuilder builder) { return default!; }", false)]
    [Arguments("public static object BuildOpen(this WebApplication app) { return app; }", false)]
    [Arguments("public static WebApplication BuildOpen(this WebApplication app, int required) { return app; }", false)]
    [Arguments("public static WebApplication BuildOpen<T>(this WebApplication app) { return app; }", false)]
    [Arguments("private static WebApplication BuildOpen(this WebApplication app) { return app; }", false)]
    [Arguments("extension(WebApplicationBuilder builder) { public WebApplication BuildOpen() { return default!; } }", false)]
    [Arguments("public static class Nested { public static WebApplication BuildOpen(WebApplication app) { return app; } }", false)]
    public async Task GivenAnExistingMethodThenOnlyCallableApplicationBuildSignaturesAreRecognized(string members, bool expected)
    {
        // Arrange
        const string methodName = "BuildOpen";
        const string extensionName = "WebApplicationExtensions";
        string source = $$"""
            namespace {{AssemblyName}};

            using Microsoft.AspNetCore.Builder;

            public static class {{extensionName}}
            {
                {{members}}
            }
            """;
        string trustedAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty;
        MetadataReference[] references = trustedAssemblies.Split(Path.PathSeparator)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        INamedTypeSymbol extensions = compilation.GetTypeByMetadataName($"{AssemblyName}.{extensionName}")!;

        // Act
        bool result = extensions.HasApplicationExtension(methodName, ApplicationType, compilation);

        // Assert
        _ = await Assert.That(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)).IsEmpty();
        _ = await Assert.That(result).IsEqualTo(expected);
    }
}