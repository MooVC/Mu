namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string ServicesNamespace = "Mu.Modelling.Services";

        public static bool IsTransform(this INamedTypeSymbol type)
        {
            INamedTypeSymbol definition = type.OriginalDefinition;

            return definition.MetadataName == "ITransform`2"
                && definition.ContainingNamespace.ToDisplayString() == ServicesNamespace;
        }
    }
}