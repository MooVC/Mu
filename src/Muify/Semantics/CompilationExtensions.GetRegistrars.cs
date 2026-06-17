namespace Muify.Semantics
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class CompilationExtensions
    {
        private const string CompositionNamespace = "Mu.Composition";
        private const string RegistrarInterfaceMetadataName = "IRegistrar";

        private static ImmutableArray<Qualification> GetRegistrars(this INamespaceSymbol namespaceSymbol)
        {
            return namespaceSymbol
                .GetTypeMembers()
                .Where(type => type.TypeKind == TypeKind.Class
                    && type.AllInterfaces.Any(@interface => @interface.IsRegistrar()))
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .Select(type => type.ToQualification())
                .ToImmutableArray();
        }

        private static bool IsRegistrar(this INamedTypeSymbol type)
        {
            INamedTypeSymbol definition = type.OriginalDefinition;

            return definition.MetadataName == RegistrarInterfaceMetadataName
                && definition.ContainingNamespace.ToDisplayString() == CompositionNamespace;
        }
    }
}