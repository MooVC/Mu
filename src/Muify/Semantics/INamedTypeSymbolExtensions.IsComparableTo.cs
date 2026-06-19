namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string ComparableInterfaceMetadataName = "IComparable`1";
        private const string SystemNamespaceName = "System";

        internal static bool IsComparableTo(this INamedTypeSymbol symbol, ITypeSymbol comparedType)
        {
            INamedTypeSymbol definition = symbol.OriginalDefinition;

            return definition.MetadataName == ComparableInterfaceMetadataName
                && definition.ContainingNamespace.ToDisplayString() == SystemNamespaceName
                && symbol.TypeArguments.Length == 1
                && SymbolEqualityComparer.Default.Equals(symbol.TypeArguments[0], comparedType);
        }
    }
}