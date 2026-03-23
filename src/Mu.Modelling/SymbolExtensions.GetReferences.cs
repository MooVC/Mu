namespace Mu.Modelling;

using System.Collections.Immutable;
using MooVC.Syntax;
using MooVC.Syntax.CSharp;

public static partial class SymbolExtensions
{
    internal static ImmutableArray<Directive> GetReferences(this IEnumerable<Symbol> symbols, Qualifier source)
    {
        return [.. symbols
            .Select(symbol => symbol.Qualifier)
            .Distinct()
            .Where(qualifier => qualifier != source)
            .OrderBy(qualifier => qualifier)
            .Select(qualifier => (Directive)qualifier)];
    }
}