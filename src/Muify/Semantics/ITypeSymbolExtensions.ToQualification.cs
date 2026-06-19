namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class ITypeSymbolExtensions
    {
        internal static Qualification ToQualification(this ITypeSymbol type)
        {
            if (type.ContainingNamespace is null || type.ContainingNamespace.IsGlobalNamespace)
            {
                return type.Name;
            }

            return (Name: type.Name, Qualifier: type.ContainingNamespace.ToDisplayString());
        }
    }
}