namespace Mu.Modelling.Syntax.CSharp
{
    using System.Collections.Generic;
    using System.Linq;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.CSharp;
    using static Mu.Modelling.Syntax.CSharp.RecordExtensions_Resources;
    using Attribute = Mu.Modelling.Attribute;

    public static partial class StructExtensions
    {
        public static Struct WithProperties(this Struct @struct, IEnumerable<Attribute> attributes)
        {
            _ = Guard.Against.Null(attributes, message: "The attributes for the properties must be provided.");
            _ = Guard.Against.Null(@struct, message: "The struct to which the Properties are applied must be provided.");

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