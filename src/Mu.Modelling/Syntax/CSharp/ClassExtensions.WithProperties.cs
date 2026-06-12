namespace Mu.Modelling.Syntax.CSharp
{
    using System.Collections.Generic;
    using System.Linq;
    using Ardalis.GuardClauses;
    using MooVC;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling.Components.Syntax.CSharp;
    using Attribute = Mu.Modelling.Attribute;

    public static partial class ClassExtensions
    {
        public static Class WithProperties(this Class @class, IEnumerable<Attribute> attributes)
        {
            _ = Guard.Against.Null(@class, message: WithPropertiesClassRequired);
            _ = Guard.Against.Null(attributes, message: WithPropertiesAttributesRequired);

            return @class.Enumerate(
                (attribute, subject) => subject.WithProperties(property => property.From(attribute)),
                attributes.OrderBy(attribute => attribute.Name));
        }
    }
}