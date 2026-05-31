namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string ImplicitConversionMetadataName = "op_Implicit";

        private static bool HasImplicitConversionTo(this INamedTypeSymbol symbol, ITypeSymbol targetType)
        {
            return symbol
                .GetMembers(ImplicitConversionMetadataName)
                .OfType<IMethodSymbol>()
                .Any(method => method.IsStatic
                    && method.MethodKind == MethodKind.Conversion
                    && method.Parameters.Length == 1
                    && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, symbol)
                    && SymbolEqualityComparer.Default.Equals(method.ReturnType, targetType));
        }
    }
}