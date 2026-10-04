namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;
    using Muify.Modelling;

    internal static partial class ITypeSymbolExtensions
    {
        public static Component CatalogValue(this ITypeSymbol value)
        {
            IPropertySymbol[] properties = value.GetProperties();

            Component component = Component.Undefined
                .AttributedWith(properties)
                .Named(value.Name);

            return value.IsPartial() || (value is INamedTypeSymbol binder && binder.HasBinder())
                ? component.WithMetadata(metadata => metadata
                    .HasBinder(value is INamedTypeSymbol named && named.HasBinder())
                    .WithCharacteristics(value.GetCharacteristics()))
                : component;
        }
    }
}