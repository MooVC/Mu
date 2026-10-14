namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static bool HasContainerExtension(this INamedTypeSymbol extensions, string name)
        {
            return extensions is object
                && (extensions.GetMembers(name).OfType<IMethodSymbol>().Any(method => method.DeclaredAccessibility == Accessibility.Public)
                    || extensions.GetTypeMembers().Where(type => type.IsImplicitlyDeclared).Any(type => type.HasContainerExtension(name)));
        }
    }
}