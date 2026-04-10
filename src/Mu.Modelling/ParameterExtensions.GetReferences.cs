namespace Mu.Modelling;

using System.Collections.Immutable;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;

public static partial class ParameterExtensions
{
    internal static ImmutableArray<Directive> GetReferences(this IEnumerable<Parameter> parameters, Qualifier source)
    {
        return [.. parameters
            .SelectMany(parameter => parameter.Type)
            .Distinct()
            .Where(qualifier => qualifier != source)
            .OrderBy(qualifier => qualifier)
            .Select(qualifier => (Directive)qualifier)];
    }
}