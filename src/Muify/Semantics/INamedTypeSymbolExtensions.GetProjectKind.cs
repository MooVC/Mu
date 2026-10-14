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
            if (definition is null || !definition.IsRecord || definition.Arity != 0 || definition.ContainingType is object)
            {
                return ProjectKind.Undefined;
            }

            INamedTypeSymbol[] attributes = definition.GetAttributes()
                .Select(attribute => attribute.AttributeClass)
                .Where(attribute => attribute is object)
                .ToArray();

            if (definition.HasAggregateBase() || attributes.Any(attribute => attribute.IsUnitAttribute()
                && attribute.ContainingNamespace.ToDisplayString() == "Muify.Domain"))
            {
                return ProjectKind.Unit;
            }

            if (definition.HasCausalBase() || attributes.Any(attribute => attribute.IsMutationalAttribute()
                || (attribute.Arity == 0
                    && attribute.Name == $"{NonMutationalStrategy.Name}Attribute"
                    && attribute.ContainingNamespace.ToDisplayString() == typeof(NonMutationalStrategy).Namespace)))
            {
                return ProjectKind.Feature;
            }

            return ProjectKind.Undefined;
        }
    }
}