namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        internal static bool ImplementsComparableTo(this ITypeSymbol symbol, ITypeSymbol comparedType)
        {
            return symbol is INamedTypeSymbol named
                && named.AllInterfaces.Any(@interface => @interface.IsComparableTo(comparedType));
        }
    }
}