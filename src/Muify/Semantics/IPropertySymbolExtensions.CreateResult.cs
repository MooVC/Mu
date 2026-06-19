namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;
    using Muify.Modelling;

    internal static partial class IPropertySymbolExtensions
    {
        internal static Feature CreateResult(this IPropertySymbol result, Feature feature)
        {
            return feature.Returning(member => member
                .Named(result.Name)
                .OfType(result.Type));
        }
    }
}