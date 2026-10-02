namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static Qualification GetImplementation(this INamedTypeSymbol request, string metadataName, string qualifier)
        {
            INamedTypeSymbol implementation = request.ContainingNamespace.GetAllTypes()
                .Where(type => type.TypeKind == TypeKind.Class && !type.IsAbstract && !type.IsGenericType)
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .FirstOrDefault(type => type.AllInterfaces.Any(@interface =>
                    @interface.OriginalDefinition.MetadataName == metadataName
                    && @interface.ContainingNamespace.ToDisplayString() == qualifier
                    && @interface.TypeArguments.Length == 2
                    && SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[0], request)));

            return implementation is null ? Qualification.Unnamed : implementation.ToQualification();
        }
    }
}