namespace Muify
{
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using static MooVC.Syntax.CSharp.Symbol;
    using Options = MooVC.Syntax.CSharp.Options;

    internal static class Configuration
    {
        public static Options Options { get; } = new Options()
            .WithNamespace(Qualifier.Options.Block)
            .WithTypes(types => types.WithQualification(Qualification.Global));
    }
}