namespace Mu.Modelling.Syntax.CSharp;

using System.ComponentModel;
using MooVC;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;
using MooVC.Syntax.Resource;
using Attribute = Mu.Modelling.Attribute;
using Modelling = Mu.Modelling.Parameter;
using Result = Mu.Modelling.Result;

internal static partial class ParameterExtensions
{
    public static Parameter From(this Parameter parameter, Attribute attribute)
    {
        return parameter.From(attribute.Description, attribute.Name, attribute.Type);
    }

    public static Parameter From(this Parameter parameter, Modelling source)
    {
        return parameter.From(source.Description, source.Name, source.Type);
    }

    public static Parameter From(this Parameter parameter, Result result)
    {
        return parameter.From(result.Description, result.Name, result.Type);
    }

    private static Parameter From(this Parameter parameter, Description description, Name name, Symbol type)
    {
        return parameter
            .ForkOn(
                _ => description.IsUndescribed,
                @true: _ => _,
                @false: parameter => parameter
                    .AttributedWith(attribute => attribute
                        .Named(typeof(DescriptionAttribute))
                        .WithArguments((Name: string.Empty, Value: $"\"{description}\""))))
            .Named(name)
            .OfType(type.AsPreferred());
    }
}