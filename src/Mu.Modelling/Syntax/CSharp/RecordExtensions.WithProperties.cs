namespace Mu.Modelling.Syntax.CSharp
{
    using System.Collections.Generic;
    using System.Linq;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.CSharp;
    using Parameter = Mu.Modelling.Parameter;

    public static partial class RecordExtensions
    {
        public static Record WithProperties(this Record record, IEnumerable<Parameter> parameters)
        {
            _ = Guard.Against.Null(record, message: WithPropertiesRecordRequired);
            _ = Guard.Against.Null(parameters, message: WithPropertiesParametersRequired);

            return record.Enumerate(
                (current, subject) => subject
                    .WithProperties(property => property
                        .From(current)
                        .WithBehaviours(behaviours => behaviours
                            .WithSet(setter => setter
                                .WithMode(Property.Methods.Setter.Modes.Init)))),
                parameters.OrderBy(parameter => parameter.Name));
        }
    }
}