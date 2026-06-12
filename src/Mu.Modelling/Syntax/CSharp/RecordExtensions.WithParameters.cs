namespace Mu.Modelling.Syntax.CSharp
{
    using System.Collections.Generic;
    using System.Linq;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.CSharp;
    using Attribute = Mu.Modelling.Attribute;
    using Parameter = Mu.Modelling.Parameter;
    using Result = Mu.Modelling.Result;

    public static partial class RecordExtensions
    {
        public static Record WithParameters(this Record record, IEnumerable<Attribute> attributes)
        {
            _ = Guard.Against.Null(record, message: WithParametersRecordRequired);
            _ = Guard.Against.Null(attributes, message: WithParametersAttributesRequired);

            return record.Enumerate(
                (current, subject) => subject.WithParameters(parameter => parameter.From(current)),
                attributes.OrderBy(attribute => attribute.Name));
        }

        public static Record WithParameters(this Record record, IEnumerable<Parameter> parameters)
        {
            _ = Guard.Against.Null(record, message: WithParametersRecordRequired);
            _ = Guard.Against.Null(parameters, message: WithParametersParametersRequired);

            return record.Enumerate(
                (current, subject) => subject.WithParameters(parameter => parameter.From(current)),
                parameters.OrderBy(parameter => parameter.Name));
        }

        public static Record WithParameters(this Record record, IEnumerable<Result> results)
        {
            _ = Guard.Against.Null(record, message: WithParametersRecordRequired);
            _ = Guard.Against.Null(results, message: WithParametersResultsRequired);

            return record.Enumerate(
                (current, subject) => subject.WithParameters(parameter => parameter.From(current)),
                results.OrderBy(result => result.Name));
        }
    }
}