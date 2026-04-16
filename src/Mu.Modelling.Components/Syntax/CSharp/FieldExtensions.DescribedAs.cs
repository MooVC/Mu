namespace Mu.Modelling.Components.Syntax.CSharp;

using System.ComponentModel;
using MooVC;
using MooVC.Syntax.CSharp;

internal static partial class FieldExtensions
{
    public static Field DescribedAs(this Field field, Description description)
    {
        return field.ForkOn(
            _ => description.IsUndescribed,
            @true: _ => _,
            @false: field => field.AttributedWith(
                typeof(DescriptionAttribute),
                attribute => attribute.WithArguments((Name: string.Empty, Value: $"\"{description}\""))));
    }
}