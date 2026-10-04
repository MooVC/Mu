namespace Muify.Semantics
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static ImmutableArray<Qualification> GetInvariants(this INamedTypeSymbol request, ITypeSymbol aggregate)
        {
            return request.ContainingNamespace
                .GetAllTypes()
                .Where(type => type.TypeKind == TypeKind.Class && !type.IsAbstract && !type.IsGenericType
                    && type.AllInterfaces.Any(@interface => @interface.IsInvariant()
                        && (aggregate is null || SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[0], aggregate))
                        && SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[1], request)))
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .Select(type => type.ToQualification())
                .ToImmutableArray();
        }
    }
}