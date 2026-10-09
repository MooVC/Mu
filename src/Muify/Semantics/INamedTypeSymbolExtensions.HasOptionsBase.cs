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
                if (current.MetadataName == "Options"
                    && current.ContainingType is null
                    && current.ContainingNamespace.ToDisplayString() == "Mu.Configuration")
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }
    }
}