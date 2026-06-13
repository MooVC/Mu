namespace Mu.Modelling.Syntax.CSharp
{
    using System.ComponentModel;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using MooVC.Syntax.Resource;
    using static Mu.Modelling.Syntax.CSharp.ParameterExtensions_Resources;
    using Attribute = Mu.Modelling.Attribute;
    using Modelling = Mu.Modelling.Parameter;
    using Result = Mu.Modelling.Result;

    public static partial class ParameterExtensions
    {
        public static Parameter From(this Parameter parameter, Attribute attribute)
        {
            return parameter.From(attribute.Default, attribute.Description, attribute.Name, attribute.Type);
        }

        public static Parameter From(this Parameter parameter, Modelling source)
        {
            return parameter.From(source.Default, source.Description, source.Name, source.Type);
        }

        public static Parameter From(this Parameter parameter, Result result)
        {
            return parameter.From(Snippet.Empty, result.Description, result.Name, result.Type);
        }

        private static Parameter From(this Parameter parameter, Snippet @default, Description description, Variable name, Symbol type)
        {
            _ = Guard.Against.Null(parameter, message: FromParameterRequired);
            _ = Guard.Against.Null(@default, message: FromDefaultRequired);
            _ = Guard.Against.Null(description, message: FromDescriptionRequired);
            _ = Guard.Against.Null(name, message: FromNameRequired);
            _ = Guard.Against.Null(type, message: FromTypeRequired);

            return parameter
                .DefaultedTo(@default)
                .ForkOn(
                    _ => description.IsUndescribed,
                    @true: _ => _,
                    @false: subject => subject.AttributedWith(
                        typeof(DescriptionAttribute),
                        attribute => attribute.WithArguments((Name: string.Empty, Value: $"\"{description}\""))))
                .Named(name)
                .OfType(type.AsPreferred());
        }
    }
}