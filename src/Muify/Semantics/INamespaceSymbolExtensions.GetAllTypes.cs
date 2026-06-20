namespace Muify.Semantics
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamespaceSymbolExtensions
    {
        internal static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamespaceSymbol @namespace)
        {
            return @namespace
                .GetTypeMembers()
                .SelectMany(type => type.GetAllTypes())
                .Concat(@namespace
                    .GetNamespaceMembers()
                    .SelectMany(member => member.GetAllTypes()));
        }
    }
}