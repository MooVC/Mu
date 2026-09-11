namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Muify.Service;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static bool IsMutationalAttribute(this INamedTypeSymbol attribute)
        {
            INamedTypeSymbol definition = attribute?.OriginalDefinition;

            return definition is object
                && definition.TypeArguments.Length == 1
                && definition.ContainingNamespace.ToDisplayString() == typeof(CreationalAttributeStrategy).Namespace
                && (definition.Name == $"{CreationalAttributeStrategy.Name}Attribute"
                 || definition.Name == $"{TransitionalAttributeStrategy.Name}Attribute");
        }
    }
}