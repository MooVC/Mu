namespace Muify.Semantics
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static IEnumerable<ITypeSymbol> GetReferencedTypes(this INamedTypeSymbol symbol)
        {
            return symbol
                .GetProperties()
                .Select(property => property.Type)
                .GroupBy(property => property.ToDisplayString())
                .Select(group => group.First());
        }
    }
}