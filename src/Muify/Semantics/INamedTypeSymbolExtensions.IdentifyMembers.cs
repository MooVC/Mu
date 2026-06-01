namespace Muify.Semantics
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using Mu.Modelling;
    using Muify.Modelling;
    using Muify.Syntax.CSharp;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static void IdentifyMembers(this INamedTypeSymbol symbol, out Component[] components, out List[] lists, CancellationToken cancellationToken)
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

        private static Component CatalogEntity(this INamedTypeSymbol entity)
        {
            IPropertySymbol[] properties = entity.GetProperties();
            Attribute identity = properties.GetIdentity(out IPropertySymbol match);

            Component component = Component.Undefined
                .AttributedWith(properties.Except(new[] { match }))
                .IdentifiedBy(identity)
                .Named(entity.Name);

            if (match is null)
            {
                return component;
            }

            return component.WithMetadata(metadata => metadata
                .WithIdentifier(identifier => identifier
                    .HasImplicitConversion(entity.HasImplicitConversionTo(match.Type))
                    .WithComparability(entity.GetComparability(match.Type)))
                .WithSelf(self => self
                    .WithComparability(entity.GetComparability(entity))));
        }

        private static Component CatalogValue(this INamedTypeSymbol value)
        {
            IPropertySymbol[] properties = value.GetProperties();

            return Component.Undefined
                .AttributedWith(properties)
                .Named(value.Name);
        }

        private static Attribute GetIdentity(this IPropertySymbol[] properties, out IPropertySymbol identity)
        {
            identity = properties.FirstOrDefault(property => property
                .GetAttributes()
                .Any(attribute => attribute.AttributeClass.IsIdentityAttribute()));

            if (identity is null)
            {
                return Attribute.Undefined;
            }

            return Attribute.Undefined.From(identity);
        }
    }
}