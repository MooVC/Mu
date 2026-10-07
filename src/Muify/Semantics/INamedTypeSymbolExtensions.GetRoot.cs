namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static Qualification GetRoot(this INamedTypeSymbol request, ITypeSymbol aggregate)
        {
            INamedTypeSymbol implementation = request.ContainingNamespace.GetAllTypes()
                .Where(type => type.TypeKind == TypeKind.Class && !type.IsAbstract && !type.IsGenericType)
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .FirstOrDefault(type => type.AllInterfaces.Any(@interface =>
                    @interface.OriginalDefinition.MetadataName == "IRoot`2"
                    && @interface.ContainingNamespace.ToDisplayString() == "Mu.Modelling.Services"
                    && (aggregate is null || SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[0], aggregate))
                    && SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[1], request)));

            return implementation is null ? Qualification.Unnamed : implementation.ToQualification();
        }
    }
}