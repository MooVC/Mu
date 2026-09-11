namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        private const string BehaviorNamespace = "Mu.Modelling.Behavior";
        private const string UseCaseName = "UseCase";

        public static bool HasUseCaseBase(this ITypeSymbol request)
        {
            ITypeSymbol current = request.BaseType;

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