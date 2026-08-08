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