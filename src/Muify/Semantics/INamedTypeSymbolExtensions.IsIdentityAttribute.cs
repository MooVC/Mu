namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Muify.Domain;

    internal static class INamedTypeSymbolExtensions
    {
        internal static bool IsIdentityAttribute(this INamedTypeSymbol symbol)
        {
            return symbol != null
                && (symbol.Name == $"{IdentityAttributeStrategy.Name}Attribute"
                 || symbol.ToDisplayString() == $"Muify.Domain.{IdentityAttributeStrategy.Name}Attribute");
        }
    }
}