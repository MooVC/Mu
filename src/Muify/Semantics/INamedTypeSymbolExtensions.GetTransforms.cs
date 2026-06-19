namespace Muify.Semantics
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class INamedTypeSymbolExtensions
    {
        internal static ImmutableArray<Qualification> GetTransforms(this INamedTypeSymbol request)
        {
            return request
                .ContainingNamespace
                .GetTypeMembers()
                .Where(type => type.TypeKind == TypeKind.Class
                    && type.AllInterfaces.Any(@interface => @interface.IsTransform()))
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .Select(type => type.ToQualification())
                .ToImmutableArray();
        }
    }
}