namespace Mu.Modelling.Syntax.CSharp;

using MooVC.Syntax.CSharp;
using Attribute = Mu.Modelling.Attribute;

internal static partial class PropertyExtensions
{
    public static Property From(this Property property, Attribute attribute)
    {
        return property
            .Named(attribute.Name)
            .OfType(attribute.Type)
            .WithDefault(attribute.Default);
    }
}