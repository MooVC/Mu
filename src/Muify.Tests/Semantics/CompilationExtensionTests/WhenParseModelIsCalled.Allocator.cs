namespace Muify.Semantics.CompilationExtensionTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mu.Modelling;

public sealed partial class WhenParseModelIsCalled
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task GivenAReferencedUnitWhenAMatchingFeatureAllocatorIsDefinedThenTheDomainAllocatorTakesPrecedenceWhenPresent(bool hasDomainAllocator)
    {
        // Arrange
        const string domainAllocatorName = "DomainAllocator";
        const string featureAllocatorName = "FeatureAllocator";
        const string featureAssemblyName = "MooVC.Testing.Mechanics.Car.Register";
        const string domainSource = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using Muify.Domain;

            [Unit<Guid>]
            public sealed record Car;
            """;
        const string domainAllocatorSource = """
            namespace MooVC.Testing.Mechanics.Car;

            using System;
            using Mu.Modelling.Services;

            public sealed class DomainAllocator : IAllocator<Guid>;
            """;
        const string featureSource = """
            namespace MooVC.Testing.Mechanics.Car.Register;

            using System;
            using Mu.Modelling.Services;

            [Muify.Service.NonMutational]
            public sealed record Register;

            public sealed class FeatureAllocator : IAllocator<Guid>;
            """;
        var domain = CSharpCompilation.Create(
            AssemblyName,
            [
                CSharpSyntaxTree.ParseText(AttributeSource),
                CSharpSyntaxTree.ParseText(ServicesSource),
                CSharpSyntaxTree.ParseText(domainSource),
                CSharpSyntaxTree.ParseText(hasDomainAllocator ? domainAllocatorSource : string.Empty),
            ],
            _references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var compilation = CSharpCompilation.Create(
            featureAssemblyName,
            [
                CSharpSyntaxTree.ParseText(ServicesSource),
                CSharpSyntaxTree.ParseText(ClassificationAttributesSource),
                CSharpSyntaxTree.ParseText(featureSource),
            ],
            _references.Append(domain.ToMetadataReference()),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Service expected = Service.Undefined
            .HasRegistrar(false)
            .WithDefinition((
                Name: hasDomainAllocator ? domainAllocatorName : featureAllocatorName,
                Qualifier: hasDomainAllocator ? AssemblyName : featureAssemblyName));

        // Act
        Service result = compilation.ParseModel(CancellationToken.None).Areas[0].Units[0].Metadata.Allocator;

        // Assert
        _ = await Assert.That(result).IsEqualTo(expected);
    }
}