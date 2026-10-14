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

            if (extensions is null
             || receiverType is null
             || applicationType is null
             || extensions.Arity != 0
             || !compilation.IsSymbolAccessibleWithin(extensions, compilation.Assembly))
            {
                return false;
            }

            return extensions.GetMembers(name).OfType<IMethodSymbol>().Any(method =>
                method.DeclaredAccessibility == Accessibility.Public
                    && method.Arity == 0
                    && !method.ReturnsByRef
                    && !method.ReturnsByRefReadonly
                    && SymbolEqualityComparer.Default.Equals(method.ReturnType, applicationType)
                    && compilation.IsSymbolAccessibleWithin(method, compilation.Assembly)
                    && (extensions.IsExtension
                        ? !method.IsStatic
                            && extensions.ExtensionParameter?.RefKind == RefKind.None
                            && SymbolEqualityComparer.Default.Equals(extensions.ExtensionParameter?.Type, receiverType)
                            && method.Parameters.All(parameter => parameter.IsOptional)
                        : method.IsStatic
                            && method.Parameters.Length > 0
                            && method.Parameters[0].RefKind == RefKind.None
                            && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, receiverType)
                            && method.Parameters.Skip(1).All(parameter => parameter.IsOptional)))
                || extensions.GetTypeMembers().Where(type => type.IsExtension)
                    .Any(type => type.HasApplicationExtension(name, receiver, compilation));
        }
    }
}