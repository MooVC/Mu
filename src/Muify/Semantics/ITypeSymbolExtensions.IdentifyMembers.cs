namespace Muify.Semantics
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class ITypeSymbolExtensions
    {
        public static void IdentifyMembers(this ITypeSymbol symbol, out Component[] components, out List[] lists, CancellationToken cancellationToken)
        {
            var entities = new List<Component>();
            var enumerations = new List<List>();
            var values = new List<Component>();

            foreach (ITypeSymbol type in symbol.GetReferencedTypes())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!(type is INamedTypeSymbol named && named.SpecialType == SpecialType.None))
                {
                    continue;
                }

                if (named.IsRecord)
                {
                    values.Add(named.CatalogValue());

                    continue;
                }

                if (!(named.TypeKind == TypeKind.Class || named.TypeKind == TypeKind.Struct))
                {
                    continue;
                }

                entities.Add(named.CatalogEntity());
            }

            components = entities
                .Concat(values)
                .ToArray();

            lists = enumerations.ToArray();
        }
    }
}