namespace Mu.Modelling.Components.Syntax.CSharp
{
    using System.ComponentModel;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.CSharp;

    public static partial class StructExtensions
    {
        public static Struct DescribedAs(this Struct @struct, Description description)
        {
            _ = Guard.Against.Null(@struct, message: DescribedAsStructRequired);
            _ = Guard.Against.Null(description, message: DescribedAsDescriptionRequired);

            return @struct.ForkOn(
                _ => description.IsUndescribed,
                @true: _ => _,
                @false: @class => @class.AttributedWith(
                    typeof(DescriptionAttribute),
                    attribute => attribute.WithArguments((Name: string.Empty, Value: $"\"{description}\""))));
        }
    }
}