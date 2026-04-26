namespace Muify.Modelling
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;
    using Muify.Semanrtics;

    internal static partial class ResultExtensions
    {
        public static Result OfType(this Result result, ITypeSymbol type)
        {
            return result.OfType(type.ToSyntax());
        }
    }
}