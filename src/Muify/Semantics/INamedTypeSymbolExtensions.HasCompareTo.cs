namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string CompareToMethodName = "CompareTo";

        private static bool HasCompareTo(this INamedTypeSymbol symbol, ITypeSymbol parameterType)
        {
            INamedTypeSymbol current = symbol;

            while (current is object)
            {
                if (current
                    .GetMembers(CompareToMethodName)
                    .OfType<IMethodSymbol>()
                    .Any(method => method.DeclaredAccessibility != Accessibility.Private
                        && !method.IsStatic
                        && method.Parameters.Length == 1
                        && method.ReturnType.SpecialType == SpecialType.System_Int32
                        && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, parameterType)))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }
    }
}