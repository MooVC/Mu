namespace Muify.Semanrtics
{
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class ITypeSymbolExtensions
    {
        public static Symbol ToSyntax(this ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol array)
            {
                return array.ElementType
                    .ToSyntax()
                    .IsArray(true)
                    .IsNullable(type.NullableAnnotation == NullableAnnotation.Annotated);
            }

            if (type is INamedTypeSymbol named)
            {
                return ToSyntax(named)
                    .IsNullable(type.NullableAnnotation == NullableAnnotation.Annotated);
            }

            return ToSymbol(type)
                .IsNullable(type.NullableAnnotation == NullableAnnotation.Annotated);
        }

        private static Symbol ToSyntax(INamedTypeSymbol type)
        {
            Symbol symbol = ToSymbol(type);

            foreach (ITypeSymbol argument in type.TypeArguments)
            {
                symbol = symbol.WithArguments(argument.ToSyntax());
            }

            return symbol;
        }

        private static Symbol ToSymbol(ITypeSymbol type)
        {
            if (type.ContainingNamespace is null || type.ContainingNamespace.IsGlobalNamespace)
            {
                return Symbol.Undefined.Named(type.Name);
            }

            return Symbol.Undefined.Named((Moniker: type.Name, Qualifier: type.ContainingNamespace.ToDisplayString()));
        }
    }
}