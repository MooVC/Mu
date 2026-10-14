namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Muify.Service;

    internal static partial class INamedTypeSymbolExtensions
    {
        internal enum ProjectKind
        {
            Undefined,
            Feature,
            Unit,
        }

        public static ProjectKind GetProjectKind(this INamedTypeSymbol definition)
        {
            if (!IsProjectRecord(definition))
            {
                return ProjectKind.Undefined;
            }

            INamedTypeSymbol[] attributes = definition.GetAttributes()
                .Select(attribute => attribute.AttributeClass)
                .Where(attribute => attribute is object)
                .ToArray();

            if (definition.HasAggregateBase() || attributes.Any(IsUnitRoleAttribute))
            {
                return ProjectKind.Unit;
            }

            if (definition.HasCausalBase() || attributes.Any(IsFeatureRoleAttribute))
            {
                return ProjectKind.Feature;
            }

            return ProjectKind.Undefined;
        }

        private static bool IsFeatureRoleAttribute(INamedTypeSymbol attribute)
        {
            return attribute.IsMutationalAttribute() || IsNonMutationalRoleAttribute(attribute);
        }

        private static bool IsNonMutationalRoleAttribute(INamedTypeSymbol attribute)
        {
            return attribute.Arity == 0
                && attribute.Name == $"{NonMutationalStrategy.Name}Attribute"
                && attribute.ContainingNamespace.ToDisplayString() == typeof(NonMutationalStrategy).Namespace;
        }

        private static bool IsProjectRecord(INamedTypeSymbol definition)
        {
            return definition is object
                && definition.IsRecord
                && definition.Arity == 0
                && definition.ContainingType is null;
        }

        private static bool IsUnitRoleAttribute(INamedTypeSymbol attribute)
        {
            return attribute.IsUnitAttribute()
                && attribute.ContainingNamespace.ToDisplayString() == "Muify.Domain";
        }
    }
}