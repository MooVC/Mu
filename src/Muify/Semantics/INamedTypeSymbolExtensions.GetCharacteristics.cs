namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class INamedTypeSymbolExtensions
    {
        public static Characteristics GetCharacteristics(this INamedTypeSymbol symbol)
        {
            return Characteristics.OutOfScope
                .IsClass(symbol.TypeKind == TypeKind.Class)
                .IsInterface(symbol.TypeKind == TypeKind.Interface)
                .IsRecord(symbol.IsRecord)
                .IsReadOnly(symbol.IsReadOnly)
                .IsRef(symbol.IsRefLikeType)
                .IsStruct(symbol.TypeKind == TypeKind.Struct);
        }
    }
}