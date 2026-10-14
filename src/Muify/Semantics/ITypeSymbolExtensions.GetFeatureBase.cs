namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        private const int CreationalOrQueryBaseArity = 1;
        private const int TransitionalBaseArity = 2;

        public static INamedTypeSymbol GetFeatureBase(this ITypeSymbol request)
        {
            INamedTypeSymbol current = request.BaseType;

            while (current is object)
            {
                if (IsFeatureBaseType(current))
                {
                    return current;
                }

                current = current.BaseType;
            }

            return default;
        }

        private static bool IsCreationalOrQueryBase(INamedTypeSymbol type)
        {
            return (type.Name == "Creational" || type.Name == "Query") && type.Arity == CreationalOrQueryBaseArity;
        }

        private static bool IsFeatureBaseType(INamedTypeSymbol type)
        {
            if (type.Arity != CreationalOrQueryBaseArity && type.Arity != TransitionalBaseArity)
            {
                return false;
            }

            return type.ContainingNamespace.ToDisplayString() == BehaviorNamespace
                && (IsCreationalOrQueryBase(type) || IsTransitionalBase(type));
        }

        private static bool IsTransitionalBase(INamedTypeSymbol type)
        {
            return type.Name == "Transitional" && type.Arity == TransitionalBaseArity;
        }
    }
}