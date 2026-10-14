namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using ModellingResult = Mu.Modelling.Result;

    internal static partial class ITypeSymbolExtensions
    {
        public static INamedTypeSymbol GetResultType(this ITypeSymbol request)
        {
            return request
                .GetTypeMembers()
                .FirstOrDefault(type => type.IsRecord && type.Name == typeof(ModellingResult).Name);
        }
    }
}