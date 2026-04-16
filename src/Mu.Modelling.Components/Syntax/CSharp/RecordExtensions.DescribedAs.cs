namespace Mu.Modelling.Components.Syntax.CSharp;

using System.ComponentModel;
using MooVC;
using MooVC.Syntax.CSharp;

internal static partial class RecordExtensions
{
    public static Record DescribedAs(this Record record, Description description)
    {
        return record.ForkOn(
            _ => description.IsUndescribed,
            @true: _ => _,
            @false: record => record.AttributedWith(
                typeof(DescriptionAttribute),
                attribute => attribute.WithArguments((Name: string.Empty, Value: $"\"{description}\""))));
    }
}