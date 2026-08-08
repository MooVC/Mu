namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string SerializationNamespace = "Mu.Serialization";
        private const string BinderInterfaceMetadataName = "IBinder";

        internal static bool IsBinder(this INamedTypeSymbol type)
        {
            INamedTypeSymbol definition = type.OriginalDefinition;

            return definition.MetadataName == BinderInterfaceMetadataName
                && definition.ContainingNamespace.ToDisplayString() == SerializationNamespace;
        }
    }
}