namespace Muify.Semanrtics
{
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class INamedTypeSymbolExtensions
    {
        internal static Symbol ToSyntax(this INamedTypeSymbol type)
        {
            var symbol = type.ToSymbol();

            foreach (ITypeSymbol argument in type.TypeArguments)
            {
                symbol = symbol.WithArguments(argument.ToSyntax());
            }

            return symbol;
        }
    }
}