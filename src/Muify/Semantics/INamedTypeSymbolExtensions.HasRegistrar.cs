namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        internal static bool HasRegistrar(this INamedTypeSymbol type)
        {
            return type.AllInterfaces.Any(@interface => @interface.IsRegistrar());
        }
    }
}