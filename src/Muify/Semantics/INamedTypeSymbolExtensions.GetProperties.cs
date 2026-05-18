namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static IPropertySymbol[] GetProperties(this INamedTypeSymbol symbol)
        {
            return GetProperties(symbol, property => property.SetMethod is object);
        }

        public static IPropertySymbol[] GetProperties(this INamedTypeSymbol symbol, Func<IPropertySymbol, bool> predicate)
        {
            return symbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(predicate)
                .ToArray();
        }
    }
}