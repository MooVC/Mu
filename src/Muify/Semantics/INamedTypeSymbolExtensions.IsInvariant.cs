namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string IntegrityNamespace = "Mu.Modelling.Integrity";

        public static bool IsInvariant(this INamedTypeSymbol type)
        {
            INamedTypeSymbol definition = type.OriginalDefinition;

            return definition.MetadataName == "IInvariant`2"
                && definition.ContainingNamespace.ToDisplayString() == IntegrityNamespace;
        }
    }
}