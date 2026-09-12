namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static bool HasConstructors(this INamedTypeSymbol type)
        {
            return type.InstanceConstructors.Any(constructor => !constructor.IsImplicitlyDeclared);
        }
    }
}