namespace Muify.Semanrtics
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

        private static Symbol ToSyntax(this INamedTypeSymbol type)
        {
            var symbol = type.ToSymbol();

            foreach (ITypeSymbol argument in type.TypeArguments)
            {
                symbol = symbol.WithArguments(argument.ToSyntax());
            }

            return symbol;
        }

        private static Symbol ToSymbol(this ITypeSymbol type)
        {
            if (type.ContainingNamespace is null || type.ContainingNamespace.IsGlobalNamespace)
            {
                return Symbol.Undefined.Named(type.Name);
            }

            return Symbol.Undefined.Named((Moniker: type.Name, Qualifier: type.ContainingNamespace.ToDisplayString()));
        }
    }
}