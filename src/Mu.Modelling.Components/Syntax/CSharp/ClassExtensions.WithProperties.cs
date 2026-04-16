namespace Mu.Modelling.Components.Syntax.CSharp;

using MooVC;
using MooVC.Syntax.CSharp;
using Mu.Modelling.Components.Syntax.CSharp;
using Attribute = Mu.Modelling.Attribute;

internal static partial class ClassExtensions
{
    public static Class WithProperties(this Class @class, IEnumerable<Attribute> attributes)
    {
        return @class.Enumerate(
            (attribute, @class) => @class.WithProperties(property => property.From(attribute)),
            attributes.OrderBy(attribute => attribute.Name));
    }
}