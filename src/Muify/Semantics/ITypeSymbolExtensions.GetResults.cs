namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using ModellingResult = Mu.Modelling.Result;

    internal static partial class ITypeSymbolExtensions
    {
        public static IPropertySymbol[] GetResults(this ITypeSymbol request)
        {
            INamedTypeSymbol result = request
                .GetTypeMembers()
                .FirstOrDefault(type => type.IsRecord && type.Name == nameof(ModellingResult));

            if (result is null)
            {
                return Array.Empty<IPropertySymbol>();
            }

            return result
                .GetMembers()
                .OfType<IPropertySymbol>()
                .ToArray();
        }
    }
}