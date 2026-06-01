namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string ComparableInterfaceMetadataName = "IComparable`1";
        private const string CompareToMethodName = "CompareTo";
        private const string GreaterThanOperatorMetadataName = "op_GreaterThan";
        private const string GreaterThanOrEqualOperatorMetadataName = "op_GreaterThanOrEqual";
        private const string LessThanOperatorMetadataName = "op_LessThan";
        private const string LessThanOrEqualOperatorMetadataName = "op_LessThanOrEqual";
        private const string SystemNamespaceName = "System";

        public static Component.Semantics.Comparability GetComparability(this INamedTypeSymbol symbol, ITypeSymbol type)
        {
            if (!type.ImplementsComparableTo(type))
            {
                return Component.Semantics.Comparability.OutOfScope;
            }

            Presence isComparable = symbol.ImplementsComparableTo(type)
                ? Presence.Present
                : Presence.Missing;

            return Component.Semantics.Comparability.OutOfScope
                .HasCompareTo(symbol.HasCompareTo(type))
                .HasGreaterThanOperator(symbol.HasOperator(GreaterThanOperatorMetadataName, type))
                .HasGreaterThanOrEqualOperator(symbol.HasOperator(GreaterThanOrEqualOperatorMetadataName, type))
                .HasLessThanOperator(symbol.HasOperator(LessThanOperatorMetadataName, type))
                .HasLessThanOrEqualOperator(symbol.HasOperator(LessThanOrEqualOperatorMetadataName, type))
                .IsComparable(isComparable);
        }

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

        private static bool ImplementsComparableTo(this ITypeSymbol symbol, ITypeSymbol comparedType)
        {
            return symbol is INamedTypeSymbol named
                && named.AllInterfaces.Any(@interface => @interface.IsComparableTo(comparedType));
        }

        private static bool IsComparableTo(this INamedTypeSymbol symbol, ITypeSymbol comparedType)
        {
            INamedTypeSymbol definition = symbol.OriginalDefinition;

            return definition.MetadataName == ComparableInterfaceMetadataName
                && definition.ContainingNamespace.ToDisplayString() == SystemNamespaceName
                && symbol.TypeArguments.Length == 1
                && SymbolEqualityComparer.Default.Equals(symbol.TypeArguments[0], comparedType);
        }
    }
}