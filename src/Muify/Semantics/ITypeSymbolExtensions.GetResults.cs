namespace Muify.Semantics
{
    using System;
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        public static IPropertySymbol[] GetResults(this ITypeSymbol request)
        {
            INamedTypeSymbol result = request.GetResultType();

            if (result is null)
            {
                return Array.Empty<IPropertySymbol>();
            }

            return result.GetProperties(IsBindableReferenceProperty);
        }
    }
}