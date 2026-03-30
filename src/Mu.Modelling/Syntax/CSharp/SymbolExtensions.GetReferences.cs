namespace Mu.Modelling.Syntax.CSharp;

using System.Collections.Immutable;
using MooVC.Linq;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;

internal static partial class SymbolExtensions
{
    public static ImmutableArray<Directive> GetReferences(this IEnumerable<Symbol> symbols, Qualifier source)
    {
        var qualifiers = new HashSet<Qualifier>();

        foreach (Symbol symbol in symbols)
        {
            symbol.GetReferences(qualifiers, source);
        }

        return [.. qualifiers.Select(qualifier => (Directive)qualifier)];
    }

    private static void GetReferences(this Symbol symbol, HashSet<Qualifier> qualifiers, Qualifier source)
    {
        if (symbol.Qualifier != source)
        {
            _ = qualifiers.Add(symbol.Qualifier);
        }

        if (symbol.IsArray)
        {
            _ = qualifiers.Add(typeof(ImmutableArray));
        }

        symbol.Arguments.ForEach(argument => GetReferences(argument, qualifiers, source));
    }
}