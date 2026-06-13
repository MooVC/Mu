namespace Mu.Modelling.Syntax.CSharp
{
    using System.ComponentModel;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.CSharp;
    using static Mu.Modelling.Syntax.CSharp.RecordExtensions_Resources;

    public static partial class RecordExtensions
    {
        public static Record DescribedAs(this Record record, Description description)
        {
            _ = Guard.Against.Null(record, message: DescribedAsRecordRequired);
            _ = Guard.Against.Null(description, message: DescribedAsDescriptionRequired);

            return record.ForkOn(
                _ => description.IsUndescribed,
                @true: _ => _,
                @false: subject => subject.AttributedWith(
                    typeof(DescriptionAttribute),
                    attribute => attribute.WithArguments((Name: string.Empty, Value: $"\"{description}\""))));
        }
    }
}