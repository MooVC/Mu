namespace Muify.Semanrtics
{
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class ITypeSymbolExtensions
    {
        internal static Symbol ToSymbol(this ITypeSymbol type)
        {
            if (type.ContainingNamespace is null || type.ContainingNamespace.IsGlobalNamespace)
            {
                return Symbol.Undefined.Named(type.Name);
            }

            return Symbol.Undefined.Named((type.Name, Qualifier: type.ContainingNamespace.ToDisplayString()));
        }
    }
}