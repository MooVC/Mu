namespace Muify.Semantics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        public static IEnumerable<ITypeSymbol> GetReferencedTypes(this ITypeSymbol symbol)
        {
            string root = symbol.ContainingNamespace.ToDisplayString(format: SymbolDisplayFormat.FullyQualifiedFormat);

            return symbol
                .GetProperties()
                .Select(property => property.Type)
                .Where(type => type.ContainingNamespace?
                    .ToDisplayString(format: SymbolDisplayFormat.FullyQualifiedFormat)
                    .StartsWith(root, StringComparison.Ordinal) == true)
                .GroupBy(property => property.ToDisplayString())
                .Select(group => group.First());
        }
    }
}