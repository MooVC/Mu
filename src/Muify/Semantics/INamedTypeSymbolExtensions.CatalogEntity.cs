namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;
    using Muify.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
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
    }
}