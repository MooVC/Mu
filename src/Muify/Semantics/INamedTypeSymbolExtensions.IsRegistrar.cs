namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string CompositionNamespace = "Mu.Composition";
        private const string RegistrarInterfaceMetadataName = "IRegistrar";

        public static bool IsRegistrar(this INamedTypeSymbol type)
        {
            INamedTypeSymbol definition = type.OriginalDefinition;

            return definition.MetadataName == RegistrarInterfaceMetadataName
                && definition.ContainingNamespace.ToDisplayString() == CompositionNamespace;
        }
    }
}