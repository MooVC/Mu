namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        internal static bool HasFact(this INamedTypeSymbol request)
        {
            ITypeSymbol fact = request
                .GetAttributes()
                .Select(attribute => attribute.AttributeClass)
                .Where(attribute => attribute.IsMutationalAttribute())
                .Select(attribute => attribute.TypeArguments[0])
                .FirstOrDefault();

            return fact?.TypeKind == TypeKind.Class;
        }
    }
}