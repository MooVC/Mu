namespace Muify.Semantics
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamedTypeSymbol type)
        {
            INamedTypeSymbol[] types = type
                .GetTypeMembers()
                .SelectMany(member => member.GetAllTypes())
                .ToArray();

            return types.Concat(new[] { type });
        }
    }
}