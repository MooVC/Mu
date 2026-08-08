namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string AggregateName = "Aggregate";
        private const string StateNamespace = "Mu.Modelling.State";

        public static bool HasAggregateBase(this ITypeSymbol definition)
        {
            ITypeSymbol current = definition.BaseType;

            while (current is object)
            {
                if (current.Name == AggregateName
                    && current.ContainingNamespace.ToDisplayString() == StateNamespace)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }
    }
}