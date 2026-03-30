namespace Mu.Modelling.Syntax.CSharp;

using System.Collections.Immutable;
using MooVC.Syntax.CSharp;

internal static partial class SymbolExtensions
{
    public static Symbol AsPreferred(this Symbol symbol)
    {
        if (!symbol.IsArray)
        {
            return symbol;
        }

        Symbol wrapper = typeof(ImmutableArray<>);

        return wrapper.WithArguments(argument => argument
            .From(symbol.Qualifier)
            .IsNullable(symbol.IsNullable)
            .Named(symbol.Name));
    }
}