namespace Mu.Modelling.Syntax.CSharp
{
    using System.Collections.Immutable;
    using MooVC.Syntax.CSharp;

    public static partial class SymbolExtensions
    {
        internal static Symbol AsPreferred(this Symbol symbol)
        {
            if (!symbol.IsArray)
            {
                return symbol;
            }

            Symbol wrapper = typeof(ImmutableArray<>);

            return wrapper.WithArguments(argument => argument
                .IsNullable(symbol.IsNullable)
                .Named(symbol.Name));
        }
    }
}