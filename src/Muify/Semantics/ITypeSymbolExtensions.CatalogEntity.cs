namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;
    using Muify.Modelling;

    internal static partial class ITypeSymbolExtensions
    {
        public static Component CatalogEntity(this ITypeSymbol entity)
        {
            IPropertySymbol[] properties = entity.GetProperties();
            Attribute identity = properties.GetIdentity(out IPropertySymbol match);

            Component component = Component.Undefined
                .AttributedWith(properties.Except(new[] { match }))
                .IdentifiedBy(identity)
                .Named(entity.Name);

            if (!entity.IsPartial())
            {
                return entity is INamedTypeSymbol binder && binder.HasBinder()
                    ? component.WithMetadata(metadata => metadata
                        .WithCharacteristics(entity.GetCharacteristics())
                        .HasBinder(true))
                    : component;
            }

            component = component.WithMetadata(metadata => metadata
                .WithCharacteristics(entity.GetCharacteristics())
                .HasBinder(entity is INamedTypeSymbol named && named.HasBinder()));

            if (match is null || entity.TypeKind != TypeKind.Class)
            {
                return component;
            }

            return component.WithMetadata(metadata => metadata
                .HasEqualsOverride(entity.HasObjectOverride("Equals", SpecialType.System_Boolean, SpecialType.System_Object))
                .HasGetHashCodeOverride(entity.HasObjectOverride("GetHashCode", SpecialType.System_Int32))
                .WithIdentifier(identifier => identifier
                    .HasImplicitConversion(entity.HasImplicitConversionTo(match.Type))
                    .WithComparability(entity.GetComparability(match.Type))
                    .WithEquality(entity.GetEquality(match.Type)))
                .WithSelf(self => self
                    .WithComparability(entity.GetComparability(entity, match.Type))
                    .WithEquality(entity.GetEquality(entity))));
        }
    }
}