namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        public static bool HasCausalBase(this ITypeSymbol definition)
        {
            ITypeSymbol current = definition?.BaseType;

            while (current is object)
            {
                if (current.Name == "Causal" && current.ContainingNamespace.ToDisplayString() == BehaviorNamespace)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }
    }
}