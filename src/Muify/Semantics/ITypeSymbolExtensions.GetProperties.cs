namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        public static IPropertySymbol[] GetProperties(this ITypeSymbol symbol)
        {
            return GetProperties(symbol, property => property.SetMethod is object);
        }

        public static IPropertySymbol[] GetProperties(this ITypeSymbol symbol, Func<IPropertySymbol, bool> predicate)
        {
            return symbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(predicate)
                .ToArray();
        }
    }
}