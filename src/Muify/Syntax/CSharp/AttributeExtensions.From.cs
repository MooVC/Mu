namespace Muify.Syntax.CSharp
{
    using Microsoft.CodeAnalysis;
    using Mu.Modelling;
    using Muify.Semanrtics;

    internal static partial class AttributeExtensions
    {
        public static Attribute From(this Attribute attribute, IPropertySymbol property)
        {
            return attribute
                .Named(property.Name)
                .OfType(property.Type.ToSyntax());
        }
    }
}