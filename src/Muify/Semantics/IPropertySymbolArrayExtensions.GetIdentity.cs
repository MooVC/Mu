namespace Muify.Semantics
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;
    using Attribute = Mu.Modelling.Attribute;

    internal static partial class IPropertySymbolArrayExtensions
    {
        public static Attribute GetIdentity(this IPropertySymbol[] properties, out IPropertySymbol identity)
        {
            identity = properties.FirstOrDefault(property => property
                .GetAttributes()
                .Any(attribute => attribute.AttributeClass.IsIdentityAttribute()));

            if (identity is null)
            {
                return Attribute.Undefined;
            }

            return Attribute.Undefined.From(identity);
        }
    }
}