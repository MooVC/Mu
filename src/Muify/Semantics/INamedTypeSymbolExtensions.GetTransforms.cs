namespace Muify.Semantics
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static ImmutableArray<Qualification> GetTransforms(this INamedTypeSymbol request, ITypeSymbol aggregate, ITypeSymbol fact)
        {
            return request
                .ContainingNamespace
                .GetAllTypes()
                .Where(type => IsConcreteServiceType(type)
                    && type.AllInterfaces.Any(@interface => IsTransformInterfaceForTypes(@interface, aggregate, fact)))
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .Select(type => type.ToQualification())
                .ToImmutableArray();
        }

        private static bool IsTransformInterfaceForTypes(INamedTypeSymbol @interface, ITypeSymbol aggregate, ITypeSymbol fact)
        {
            return @interface.IsTransform()
                && (aggregate is null || SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[0], aggregate))
                && (fact is null || SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[1], fact));
        }
    }
}