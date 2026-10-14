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

            return options.InstanceConstructors.Any(constructor => constructor.Parameters.All(parameter => parameter.IsOptional)
                && (constructor.DeclaredAccessibility == Accessibility.Public
                    || constructor.DeclaredAccessibility == Accessibility.Internal
                    || constructor.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
                && (!hasRequiredMembers || constructor.GetAttributes().Any(attribute =>
                    attribute.AttributeClass?.ToDisplayString() == "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute")));
        }

        private static bool HasRequiredMembers(INamedTypeSymbol options)
        {
            for (INamedTypeSymbol current = options; current is object; current = current.BaseType)
            {
                if (current.GetMembers().Any(member => (member is IPropertySymbol property && property.IsRequired)
                    || (member is IFieldSymbol field && field.IsRequired)))
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
                if (current.Arity > 0
                    || current.IsFileLocal
                    || current.DeclaredAccessibility == Accessibility.Private
                    || current.DeclaredAccessibility == Accessibility.Protected
                    || current.DeclaredAccessibility == Accessibility.ProtectedAndInternal)
                {
                    return false;
                }

                current = current.ContainingType;
            }

            return true;
        }
    }
}