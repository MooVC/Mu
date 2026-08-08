namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;

    internal static partial class ITypeSymbolExtensions
    {
        public static Characteristics GetCharacteristics(this ITypeSymbol symbol)
        {
            return Characteristics.Undefined
                .IsClass(symbol.TypeKind == TypeKind.Class)
                .IsInterface(symbol.TypeKind == TypeKind.Interface)
                .IsRecord(symbol.IsRecord)
                .IsReadOnly(symbol.IsReadOnly)
                .IsRef(symbol.IsRefLikeType)
                .IsStruct(symbol.TypeKind == TypeKind.Struct);
        }
    }
}