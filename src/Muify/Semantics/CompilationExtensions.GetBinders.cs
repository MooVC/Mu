namespace Muify.Semantics
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class CompilationExtensions
    {
        private static ImmutableArray<Qualification> GetBinders(this Compilation compilation)
        {
            return compilation.References
                .Where(IsGlobalReference)
                .Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .SelectMany(assembly => assembly.GlobalNamespace.GetAllTypes())
                .Where(binder => IsAssemblyBinder(binder, compilation))
                .Select(binder => binder.ToQualification())
                .Distinct()
                .OrderBy(binder => $"{binder.Qualifier}.{binder.Moniker}", StringComparer.Ordinal)
                .ToImmutableArray();
        }

        private static bool HasAssemblyBinder(this Compilation compilation)
        {
            return compilation.Assembly.GlobalNamespace.GetAllTypes()
                .Any(type => IsAssemblyBinder(type, compilation));
        }

        private static bool IsAssemblyBinder(INamedTypeSymbol type, Compilation compilation)
        {
            return IsAssemblyBinderType(type)
                && (type.TypeKind == TypeKind.Class || type.TypeKind == TypeKind.Struct)
                && type.DeclaredAccessibility == Accessibility.Public
                && !type.IsGenericType
                && !type.IsAbstract
                && !type.IsRefLikeType
                && compilation.IsSymbolAccessibleWithin(type, compilation.Assembly);
        }

        private static bool IsAssemblyBinderType(INamedTypeSymbol type)
        {
            return type.ContainingType is null
                && type.Name == "Binder"
                && type.AllInterfaces.Any(@interface => @interface.IsBinder());
        }
    }
}