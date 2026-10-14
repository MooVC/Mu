namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static Poco CatalogOptions(this INamedTypeSymbol options)
        {
            return options is null
                ? Poco.Undefined
                : Poco.Undefined
                    .HasBase(options.BaseType is object && options.BaseType.SpecialType != SpecialType.System_Object)
                    .HasOptionsBase(options.HasOptionsBase())
                    .HasParameterlessConstructor(CanInitialize(options))
                    .IsAccessible(options.Arity == 0 && IsAccessible(options))
                    .IsPartial(options.IsPartial())
                    .WithCharacteristics(options.GetCharacteristics())
                    .WithQualification(options.ToQualification());
        }

        private static bool CanInitialize(INamedTypeSymbol options)
        {
            if (options.IsAbstract)
            {
                return false;
            }

            bool hasRequiredMembers = HasRequiredMembers(options);

            return options.InstanceConstructors.Any(constructor => CanInvokeOptionsConstructor(constructor, hasRequiredMembers));
        }

        private static bool CanInvokeOptionsConstructor(IMethodSymbol constructor, bool hasRequiredMembers)
        {
            return constructor.Parameters.All(parameter => parameter.IsOptional)
                && HasAccessibleOptionsConstructor(constructor)
                && (!hasRequiredMembers || SetsRequiredOptionsMembers(constructor));
        }

        private static bool HasAccessibleOptionsConstructor(IMethodSymbol constructor)
        {
            return constructor.DeclaredAccessibility == Accessibility.Public
                || constructor.DeclaredAccessibility == Accessibility.Internal
                || constructor.DeclaredAccessibility == Accessibility.ProtectedOrInternal;
        }

        private static bool HasRequiredMembers(INamedTypeSymbol options)
        {
            for (INamedTypeSymbol current = options; current is object; current = current.BaseType)
            {
                if (current.GetMembers().Any(IsRequiredOptionsMember))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAccessible(INamedTypeSymbol options)
        {
            INamedTypeSymbol current = options;

            while (current is object)
            {
                if (IsInaccessibleOptionsType(current))
                {
                    return false;
                }

                current = current.ContainingType;
            }

            return true;
        }

        private static bool IsInaccessibleOptionsType(INamedTypeSymbol options)
        {
            return options.Arity > 0
                || options.IsFileLocal
                || options.DeclaredAccessibility == Accessibility.Private
                || options.DeclaredAccessibility == Accessibility.Protected
                || options.DeclaredAccessibility == Accessibility.ProtectedAndInternal;
        }

        private static bool IsRequiredOptionsMember(ISymbol member)
        {
            return (member is IPropertySymbol property && property.IsRequired)
                || (member is IFieldSymbol field && field.IsRequired);
        }

        private static bool SetsRequiredOptionsMembers(IMethodSymbol constructor)
        {
            return constructor.GetAttributes().Any(attribute =>
                attribute.AttributeClass?.ToDisplayString() == "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute");
        }
    }
}