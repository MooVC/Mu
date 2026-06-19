namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
        private const string GreaterThanOperatorMetadataName = "op_GreaterThan";
        private const string GreaterThanOrEqualOperatorMetadataName = "op_GreaterThanOrEqual";
        private const string LessThanOperatorMetadataName = "op_LessThan";
        private const string LessThanOrEqualOperatorMetadataName = "op_LessThanOrEqual";

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
    }
}