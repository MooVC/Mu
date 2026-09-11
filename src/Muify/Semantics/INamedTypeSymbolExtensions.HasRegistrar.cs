namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static bool HasRegistrar(this INamedTypeSymbol type)
        {
            return type.AllInterfaces.Any(@interface => @interface.IsRegistrar());
        }
    }
}