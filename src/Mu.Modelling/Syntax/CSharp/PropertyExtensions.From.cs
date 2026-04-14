namespace Mu.Modelling.Syntax.CSharp;

using System.ComponentModel;
using MooVC;
using MooVC.Syntax.CSharp;
using Attribute = Mu.Modelling.Attribute;

internal static partial class PropertyExtensions
{
    public static Property From(this Property property, Attribute attribute)
    {
        return property
            .ForkOn(
                _ => attribute.Description.IsUndescribed,
                @true: _ => _,
                @false: property => property.AttributedWith(
                    typeof(DescriptionAttribute),
                    description => description.WithArguments((Name: string.Empty, Value: $"\"{attribute.Description}\""))))
            .Named(attribute.Name)
            .OfType(attribute.Type.AsPreferred())
            .WithDefault(attribute.Default);
    }
}