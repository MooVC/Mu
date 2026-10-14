namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        private const string CompareToMethodName = "CompareTo";

        public static bool HasCompareTo(this ITypeSymbol symbol, ITypeSymbol parameterType)
        {
            ITypeSymbol current = symbol;

            while (current is object)
            {
                if (current
                    .GetMembers(CompareToMethodName)
                    .OfType<IMethodSymbol>()
                    .Any(method => IsCompatibleCompareToMethod(method, parameterType)))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private static bool IsCompatibleCompareToMethod(IMethodSymbol method, ITypeSymbol parameterType)
        {
            return method.DeclaredAccessibility != Accessibility.Private
                && !method.IsStatic
                && method.Parameters.Length == 1
                && method.ReturnType.SpecialType == SpecialType.System_Int32
                && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, parameterType);
        }
    }
}