namespace Mu.Modelling.Syntax.CSharp
{
    using System.Collections.Generic;
    using System.Linq;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.CSharp;
    using static Mu.Modelling.Syntax.CSharp.StructExtensions_Resources;
    using Attribute = Mu.Modelling.Attribute;

    public static partial class StructExtensions
    {
        public static Struct WithProperties(this Struct @struct, IEnumerable<Attribute> attributes)
        {
            _ = Guard.Against.Null(attributes, message: WithPropertiesAttributesRequired);
            _ = Guard.Against.Null(@struct, message: WithPropertiesStructRequired);

            return @struct.Enumerate(
                (current, subject) => subject
                    .WithProperties(property => property
                        .From(current)
                        .WithBehaviours(behaviours => behaviours
                            .WithSet(setter => setter
                                .WithMode(Property.Methods.Setter.Modes.Init)))),
                attributes.OrderBy(parameter => parameter.Name));
        }
    }
}