namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static bool HasApplicationExtension(this INamedTypeSymbol extensions, string name, string receiver, Compilation compilation)
        {
            ITypeSymbol receiverType = compilation.GetTypeByMetadataName(receiver);
            ITypeSymbol applicationType = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Builder.WebApplication");

            if (!CanInspectApplicationExtensions(extensions, receiverType, applicationType, compilation))
            {
                return false;
            }

            return extensions.GetMembers(name).OfType<IMethodSymbol>().Any(method =>
                IsUsableApplicationExtensionMethod(method, extensions, receiverType, applicationType, compilation))
                || extensions.GetTypeMembers().Where(type => type.IsExtension)
                    .Any(type => type.HasApplicationExtension(name, receiver, compilation));
        }

        private static bool CanInspectApplicationExtensions(
            INamedTypeSymbol extensions,
            ITypeSymbol receiverType,
            ITypeSymbol applicationType,
            Compilation compilation)
        {
            return extensions is object
                && receiverType is object
                && applicationType is object
                && extensions.Arity == 0
                && compilation.IsSymbolAccessibleWithin(extensions, compilation.Assembly);
        }

        private static bool IsExtensionBlockApplicationMethod(IMethodSymbol method, INamedTypeSymbol extensions, ITypeSymbol receiverType)
        {
            return !method.IsStatic
                && extensions.ExtensionParameter?.RefKind == RefKind.None
                && SymbolEqualityComparer.Default.Equals(extensions.ExtensionParameter?.Type, receiverType)
                && method.Parameters.All(parameter => parameter.IsOptional);
        }

        private static bool IsStaticApplicationExtensionMethod(IMethodSymbol method, ITypeSymbol receiverType)
        {
            return method.IsStatic
                && method.Parameters.Length > 0
                && method.Parameters[0].RefKind == RefKind.None
                && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, receiverType)
                && method.Parameters.Skip(1).All(parameter => parameter.IsOptional);
        }

        private static bool IsUsableApplicationExtensionMethod(
            IMethodSymbol method,
            INamedTypeSymbol extensions,
            ITypeSymbol receiverType,
            ITypeSymbol applicationType,
            Compilation compilation)
        {
            return method.DeclaredAccessibility == Accessibility.Public
                && method.Arity == 0
                && !method.ReturnsByRef
                && !method.ReturnsByRefReadonly
                && SymbolEqualityComparer.Default.Equals(method.ReturnType, applicationType)
                && compilation.IsSymbolAccessibleWithin(method, compilation.Assembly)
                && (extensions.IsExtension
                    ? IsExtensionBlockApplicationMethod(method, extensions, receiverType)
                    : IsStaticApplicationExtensionMethod(method, receiverType));
        }
    }
}