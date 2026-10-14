namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static bool HasOptionsBase(this INamedTypeSymbol options)
        {
            INamedTypeSymbol current = options.BaseType;

            while (current is object)
            {
                if (IsConfigurationOptionsType(current))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private static bool IsConfigurationOptionsType(INamedTypeSymbol type)
        {
            return type.MetadataName == "Options"
                && type.ContainingType is null
                && type.ContainingNamespace.ToDisplayString() == "Mu.Configuration";
        }
    }
}