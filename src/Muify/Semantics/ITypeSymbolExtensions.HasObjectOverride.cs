namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static partial class ITypeSymbolExtensions
    {
        public static bool HasObjectOverride(this ITypeSymbol symbol, string name, SpecialType returnType, params SpecialType[] parameters)
        {
            return symbol.GetMembers(name)
                .OfType<IMethodSymbol>()
                .Any(method => method.IsOverride
                    && !method.IsStatic
                    && method.ReturnType.SpecialType == returnType
                    && method.Parameters.Select(parameter => parameter.Type.SpecialType).SequenceEqual(parameters));
        }
    }
}