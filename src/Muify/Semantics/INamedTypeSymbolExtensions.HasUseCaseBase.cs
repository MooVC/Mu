namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string BehaviorNamespace = "Mu.Modelling.Behavior";
        private const string UseCaseName = "UseCase";

        internal static bool HasUseCaseBase(this INamedTypeSymbol request)
        {
            INamedTypeSymbol current = request.BaseType;

            while (current is object)
            {
                if (current.Name == UseCaseName
                    && current.ContainingNamespace.ToDisplayString() == BehaviorNamespace)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }
    }
}