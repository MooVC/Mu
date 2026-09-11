namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static partial class ITypeSymbolExtensions
    {
        public static bool IsPartial(this ITypeSymbol symbol)
        {
            return symbol
                .DeclaringSyntaxReferences
                .Select(syntax => syntax.GetSyntax())
                .OfType<BaseTypeDeclarationSyntax>()
                .Any(HasPartialModifier);
        }

        private static bool HasPartialModifier(BaseTypeDeclarationSyntax syntax)
        {
            return syntax.Modifiers.Any(SyntaxKind.PartialKeyword);
        }
    }
}