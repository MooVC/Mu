namespace Mu.Modelling.Syntax.CSharp;

using System.ComponentModel;
using MooVC;
using MooVC.Syntax.CSharp;

internal static partial class ClassExtensions
{
    public static Class DescribedAs(this Class @class, Description description)
    {
        return @class.ForkOn(
            _ => description.IsUndescribed,
            @true: _ => _,
            @false: @class => @class.AttributedWith(
                typeof(DescriptionAttribute),
                attribute => attribute.WithArguments((Name: string.Empty, Value: $"\"{description}\""))));
    }
}