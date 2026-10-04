namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using MooVC.Syntax.CSharp;

    internal static partial class ITypeSymbolExtensions
    {
        public static Qualification ToQualification(this ITypeSymbol type)
        {
            string name = type.ContainingType is null
                ? type.Name
                : $"{type.ContainingType.ToQualification().Moniker}.{type.Name}";

            if (type.ContainingNamespace is null || type.ContainingNamespace.IsGlobalNamespace)
            {
                return name;
            }

            return (name, Qualifier: type.ContainingNamespace.ToDisplayString());
        }
    }
}