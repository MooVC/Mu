namespace Mu.Modelling.Syntax.CSharp;

using MooVC;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Syntax.CSharp;
using Attribute = Mu.Modelling.Attribute;
using Parameter = Mu.Modelling.Parameter;
using Result = Mu.Modelling.Result;

internal static partial class RecordExtensions
{
    public static Record WithParameters(this Record record, IEnumerable<Attribute> attributes)
    {
        return record.Enumerate(
            (current, record) => record.WithParameters(parameter => parameter.From(current)),
            attributes.OrderBy(attribute => attribute.Name));
    }

    public static Record WithParameters(this Record record, IEnumerable<Parameter> parameters)
    {
        return record.Enumerate(
            (current, record) => record.WithParameters(parameter => parameter.From(current)),
            parameters.OrderBy(parameter => parameter.Name));
    }

    public static Record WithParameters(this Record record, IEnumerable<Result> results)
    {
        return record.Enumerate(
            (current, record) => record.WithParameters(parameter => parameter.From(current)),
            results.OrderBy(result => result.Name));
    }
}