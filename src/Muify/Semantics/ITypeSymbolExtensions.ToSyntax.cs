namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class ITypeSymbolExtensions
    {
        public static Symbol ToSyntax(this ITypeSymbol type)
        {
            Symbol symbol;

            if (type is IArrayTypeSymbol array)
            {
                symbol = array.ElementType
                    .ToSyntax()
                    .IsArray(true);
            }
            else if (type is INamedTypeSymbol named)
            {
                symbol = named.ToSyntax();
            }
            else
            {
                symbol = type.ToSymbol();
            }

            return symbol.IsNullable(type.NullableAnnotation == NullableAnnotation.Annotated);
        }
    }
}