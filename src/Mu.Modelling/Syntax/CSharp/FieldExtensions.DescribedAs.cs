namespace Mu.Modelling.Syntax.CSharp
{
    using System.ComponentModel;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.CSharp;
    using static Mu.Modelling.Syntax.CSharp.FieldExtensions_Resources;

    public static partial class FieldExtensions
    {
        public static Field DescribedAs(this Field field, Description description)
        {
            _ = Guard.Against.Null(field, message: DescribedAsFieldRequired);
            _ = Guard.Against.Null(description, message: DescribedAsDescriptionRequired);

            return field.ForkOn(
                _ => description.IsUndescribed,
                @true: _ => _,
                @false: subject => subject.AttributedWith(
                    typeof(DescriptionAttribute),
                    attribute => attribute.WithArguments((Name: string.Empty, Value: $"\"{description}\""))));
        }
    }
}