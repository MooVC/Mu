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
                .Where(type => IsConcreteServiceType(type)
                    && type.AllInterfaces.Any(@interface => IsInvariantInterfaceForRequest(@interface, request, aggregate)))
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .Select(type => type.ToQualification())
                .ToImmutableArray();
        }

        private static bool IsInvariantInterfaceForRequest(INamedTypeSymbol @interface, INamedTypeSymbol request, ITypeSymbol aggregate)
        {
            return @interface.IsInvariant()
                && (aggregate is null || SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[0], aggregate))
                && SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[1], request);
        }
    }
}