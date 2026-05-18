namespace Muify.Semantics.CompilationExtensionTests;

using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mu.Modelling;

public sealed class WhenParseModelIsCalled
{
    private const string AssemblyName = "MooVC.Testing.Mechanics.Car";

    private const string AttributeSource = """
        namespace Muify.Domain;

        using System;

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public sealed class UnitAttribute<TIdentity>
            : Attribute
            where TIdentity : struct
        {
        }
        """;

    private static readonly MetadataReference[] _references = GetReferences();

    [Test]
    public async Task GivenAUnitAttributeThenUnitIdentityIsDiscoveredFromTheGenericArgument()
    {
        // Arrange
        const string source = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using Muify.Domain;

            [Unit<Guid>]
            public sealed partial record Car;
            """;

        Unit expected = Unit.Undefined.IdentifiedBy(typeof(Guid));

        // Act
        Model result = GetModel(source);

        // Assert
        _ = await Assert.That(result.Areas[0].Units[0].Identity).IsEqualTo(expected.Identity);
    }

    private static Model GetModel(string source)
    {
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            [
                CSharpSyntaxTree.ParseText(AttributeSource),
                CSharpSyntaxTree.ParseText(source),
            ],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return compilation.ParseModel(CancellationToken.None);
    }

    private static MetadataReference[] GetReferences()
    {
        string trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty;

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }
}