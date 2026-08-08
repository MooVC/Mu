namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Muify.Domain;

    internal static partial class ITypeSymbolExtensions
    {
        public static bool IsIdentityAttribute(this ITypeSymbol symbol)
        {
            return symbol != null
                && (symbol.Name == $"{IdentityAttributeStrategy.Name}Attribute"
                 || symbol.ToDisplayString() == $"Muify.Domain.{IdentityAttributeStrategy.Name}Attribute");
        }
    }
}