namespace Mu.Modelling.Components.Syntax.CSharp;

using System.ComponentModel;
using MooVC;
using MooVC.Syntax.CSharp;

internal static partial class StructExtensions
{
    public static Struct DescribedAs(this Struct @struct, Description description)
    {
        return @struct.ForkOn(
            _ => description.IsUndescribed,
            @true: _ => _,
            @false: @class => @class.AttributedWith(
                typeof(DescriptionAttribute),
                attribute => attribute.WithArguments((Name: string.Empty, Value: $"\"{description}\""))));
    }
}