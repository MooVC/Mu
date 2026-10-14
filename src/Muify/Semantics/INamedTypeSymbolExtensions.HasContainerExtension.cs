namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static bool HasContainerExtension(this INamedTypeSymbol extensions, string name)
        {
            return extensions is object
                && (HasPublicContainerExtension(extensions, name) || HasNestedContainerExtension(extensions, name));
        }

        private static bool HasNestedContainerExtension(INamedTypeSymbol extensions, string name)
        {
            return extensions.GetTypeMembers()
                .Where(type => type.IsImplicitlyDeclared)
                .Any(type => type.HasContainerExtension(name));
        }

        private static bool HasPublicContainerExtension(INamedTypeSymbol extensions, string name)
        {
            return extensions.GetMembers(name)
                .OfType<IMethodSymbol>()
                .Any(method => method.DeclaredAccessibility == Accessibility.Public);
        }
    }
}