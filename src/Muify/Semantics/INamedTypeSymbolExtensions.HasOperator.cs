namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        private static bool HasOperator(this INamedTypeSymbol symbol, string metadataName, ITypeSymbol operandType)
        {
            return symbol
                .GetMembers(metadataName)
                .OfType<IMethodSymbol>()
                .Any(method => method.IsStatic
                    && method.MethodKind == MethodKind.UserDefinedOperator
                    && method.Parameters.Length == 2
                    && method.ReturnType.SpecialType == SpecialType.System_Boolean
                    && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, symbol)
                    && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, operandType));
        }
    }
}