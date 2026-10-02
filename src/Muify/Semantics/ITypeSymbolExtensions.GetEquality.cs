namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class ITypeSymbolExtensions
    {
        public static Component.Semantics.Equality GetEquality(this ITypeSymbol symbol, ITypeSymbol type)
        {
            bool hasEquals = false;

            for (ITypeSymbol current = symbol; current is object; current = current.BaseType)
            {
                hasEquals |= current.GetMembers("Equals")
                    .OfType<IMethodSymbol>()
                    .Any(method => !method.IsStatic
                        && method.Arity == 0
                        && method.DeclaredAccessibility == Accessibility.Public
                        && method.ReturnType.SpecialType == SpecialType.System_Boolean
                        && method.Parameters.Length == 1
                        && method.Parameters[0].RefKind == RefKind.None
                        && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, type));
            }

            bool isEquatable = symbol.AllInterfaces.Any(@interface =>
                @interface.OriginalDefinition.MetadataName == "IEquatable`1"
                && @interface.ContainingNamespace.ToDisplayString() == "System"
                && SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[0], type));

            return Component.Semantics.Equality.OutOfScope
                .HasEquatable(hasEquals)
                .HasEqualsOperator(symbol.HasOperator("op_Equality", type))
                .HasNotEqualsOperator(symbol.HasOperator("op_Inequality", type))
                .IsEquatable(isEquatable);
        }
    }
}