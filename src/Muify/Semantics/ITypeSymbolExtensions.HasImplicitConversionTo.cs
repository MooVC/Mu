namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        private const string ImplicitConversionMetadataName = "op_Implicit";

        public static bool HasImplicitConversionTo(this ITypeSymbol symbol, ITypeSymbol targetType)
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