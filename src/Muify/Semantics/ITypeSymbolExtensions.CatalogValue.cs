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

            return Component.Undefined
                .AttributedWith(properties)
                .Named(value.Name)
                .WithMetadata(metadata => metadata.WithCharacteristics(value.GetCharacteristics()));
        }
    }
}