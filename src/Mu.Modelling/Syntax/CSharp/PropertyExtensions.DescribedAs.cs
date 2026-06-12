namespace Mu.Modelling.Syntax.CSharp
{
    using System.ComponentModel;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.CSharp;

    public static partial class PropertyExtensions
    {
        public static Property DescribedAs(this Property property, Description description)
        {
            _ = Guard.Against.Null(property, message: DescribedAsPropertyRequired);
            _ = Guard.Against.Null(description, message: DescribedAsDescriptionRequired);

            return property.ForkOn(
                _ => description.IsUndescribed,
                @true: _ => _,
                @false: subject => subject.AttributedWith(
                    typeof(DescriptionAttribute),
                    attribute => attribute.WithArguments((Name: string.Empty, Value: $"\"{description}\""))));
        }
    }
}