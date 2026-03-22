namespace Mu.Modelling;

using System.Collections.Immutable;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;

public static partial class AttributeExtensions
{
    internal static ImmutableArray<Directive> GetReferences(this IEnumerable<Attribute> attributes, Qualifier source)
    {
        return [.. attributes
            .Select(attribute => attribute.Type.Qualifier)
            .Distinct()
            .Where(qualifier => qualifier != source)
            .OrderBy(qualifier => qualifier)
            .Select(qualifier => (Directive)qualifier)];
    }
}