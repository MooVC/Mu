namespace Muify.Semantics
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class CompilationExtensions
    {
        private static ImmutableArray<Poco> GetSharedTypes(this Compilation compilation, string applicationNamespace)
        {
            return compilation.Assembly.GlobalNamespace.GetAllTypes()
                .Where(type => IsSharedType(type, applicationNamespace))
                .OrderBy(type => type.ToDisplayString(), StringComparer.Ordinal)
                .Select(type => type.CatalogReference(compilation.Assembly))
                .Where(CanBindSharedType)
                .ToImmutableArray();
        }

        private static bool CanBindSharedType(Poco type)
        {
            return type.IsAccessible && (type.HasBinder || type.IsPartial);
        }

        private static bool IsConventionalAssemblyBinder(INamedTypeSymbol type, string applicationNamespace)
        {
            return IsAssemblyBinderType(type)
                || (type.ContainingType is null
                    && type.Name == "Binder"
                    && type.ContainingNamespace.ToDisplayString() == applicationNamespace);
        }

        private static bool IsSharedType(INamedTypeSymbol type, string applicationNamespace)
        {
            if (type.IsStatic || IsConventionalAssemblyBinder(type, applicationNamespace))
            {
                return false;
            }

            for (INamedTypeSymbol subject = type; subject is object; subject = subject.ContainingType)
            {
                if (!IsSupportedSharedType(subject))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsSupportedSharedType(INamedTypeSymbol type)
        {
            return (type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Struct)
                && !type.IsGenericType
                && !type.IsRefLikeType
                && !type.IsFileLocal
                && (type.DeclaredAccessibility == Accessibility.Public || type.DeclaredAccessibility == Accessibility.Internal);
        }
    }
}