namespace Muify.Modelling
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;
    using Muify.Semantics;

    internal static partial class ResultExtensions
    {
        public static Result OfType(this Result result, ITypeSymbol type)
        {
            return result.OfType(type.ToSyntax());
        }
    }
}