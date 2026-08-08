namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Muify.Domain;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static bool IsUnitAttribute(this INamedTypeSymbol symbol)
        {
            return symbol != null
                && symbol.TypeArguments.Length == 1
                && (symbol.Name == $"{UnitAttributeStrategy.Name}Attribute"
                 || symbol.OriginalDefinition.ToDisplayString() == $"Muify.Domain.{UnitAttributeStrategy.Name}Attribute<TIdentity>");
        }
    }
}