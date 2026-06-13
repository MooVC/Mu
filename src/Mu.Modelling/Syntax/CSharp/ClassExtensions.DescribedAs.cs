namespace Mu.Modelling.Syntax.CSharp
{
    using System.ComponentModel;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.CSharp;
    using static Mu.Modelling.Syntax.CSharp.ClassExtensions_Resources;

    public static partial class ClassExtensions
    {
        public static Class DescribedAs(this Class @class, Description description)
        {
            _ = Guard.Against.Null(@class, message: DescribedAsClassRequired);
            _ = Guard.Against.Null(description, message: DescribedAsDescriptionRequired);

            return @class.ForkOn(
                _ => description.IsUndescribed,
                @true: _ => _,
                @false: subject => subject.AttributedWith(
                    typeof(DescriptionAttribute),
                    attribute => attribute.WithArguments((Name: string.Empty, Value: $"\"{description}\""))));
        }
    }
}