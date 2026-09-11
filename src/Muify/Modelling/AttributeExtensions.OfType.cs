namespace Muify.Modelling
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;
    using Muify.Semantics;

    internal static partial class AttributeExtensions
    {
        public static Attribute OfType(this Attribute attribute, ITypeSymbol type)
        {
            return attribute.OfType(type.ToSyntax());
        }
    }
}