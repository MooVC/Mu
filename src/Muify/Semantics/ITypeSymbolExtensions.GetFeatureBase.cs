namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        public static INamedTypeSymbol GetFeatureBase(this ITypeSymbol request)
        {
            INamedTypeSymbol current = request.BaseType;

            const int ExpectedArityForCreationalOrQuery = 1;
            const int ExpectedArityForTransitional = 2;

            while (current is object)
            {
                if (current.Arity == ExpectedArityForCreationalOrQuery || current.Arity == ExpectedArityForTransitional)
                {
                    string @namespace = current.ContainingNamespace.ToDisplayString();

                    bool IsCreationalOrQuery()
                    {
                        return (current.Name == "Creational" || current.Name == "Query") && current.Arity == ExpectedArityForCreationalOrQuery;
                    }

                    bool IsTransitional()
                    {
                        return current.Name == "Transitional" && current.Arity == ExpectedArityForTransitional;
                    }

                    if (@namespace == BehaviorNamespace && (IsCreationalOrQuery() || IsTransitional()))
                    {
                        return current;
                    }
                }

                current = current.BaseType;
            }

            return default;
        }
    }
}