namespace Muify
{
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Options = MooVC.Syntax.CSharp.Options;

    internal static class Configuration
    {
        public static Options Options { get; } = new Options()
            .WithNamespace(Qualifier.Options.File)
            .WithTypes(types => types
                .WithQualifications(qualifications => qualifications
                    .WithFormat(Qualification.Options.Formats.Global)));

        public static string Render(Symbol symbol)
        {
            return symbol.ToSnippet(Options.Types);
        }

        public static partial class LifeStyles
        {
            public static readonly Symbol Scoped = (Name: "Lifestyle.Scoped", Qualifier: "SimpleInjector");
            public static readonly Symbol Singleton = (Name: "Lifestyle.Singleton", Qualifier: "SimpleInjector");
            public static readonly Symbol Transient = (Name: "Lifestyle.Transient", Qualifier: "SimpleInjector");
        }
    }
}