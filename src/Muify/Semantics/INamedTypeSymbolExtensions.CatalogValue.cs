namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;
    using Muify.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
        private static Component CatalogValue(this INamedTypeSymbol value)
        {
            IPropertySymbol[] properties = value.GetProperties();

            return Component.Undefined
                .AttributedWith(properties)
                .Named(value.Name)
                .WithMetadata(metadata => metadata.WithCharacteristics(value.GetCharacteristics()));
        }
    }
}