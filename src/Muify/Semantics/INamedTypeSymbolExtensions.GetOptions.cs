namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static INamedTypeSymbol GetOptions(this INamedTypeSymbol request)
        {
            INamedTypeSymbol options = request.ContainingNamespace.GetAllTypes()
                .Where(type => SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, request.ContainingAssembly))
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .FirstOrDefault(type => type.HasOptionsBase());

            return options ?? request.ContainingNamespace
                .GetTypeMembers($"{request.Name}Options")
                .FirstOrDefault(type => type.Arity == 0
                    && SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, request.ContainingAssembly));
        }
    }
}