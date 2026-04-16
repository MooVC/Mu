namespace Mu.Modelling
{
    using Ardalis.GuardClauses;
    using MooVC.Syntax.Validation;
    using SyntaxOptions = MooVC.Syntax.CSharp.Options;

    public sealed partial class Options
    {
        public static readonly Options Default = new Options();

        public Options()
            : this(GithubOptions.Default, SyntaxOptions.Default)
        {
        }

        public Options(GithubOptions github, SyntaxOptions syntax)
        {
            Github = github;
            Syntax = syntax;
        }

        public GithubOptions Github { get; }

        public SyntaxOptions Syntax { get; }

        public static implicit operator GithubOptions(Options options)
        {
            Guard.Against.Conversion<Options, GithubOptions>(options);

            return options.Github;
        }

        public static implicit operator SyntaxOptions(Options options)
        {
            Guard.Against.Conversion<Options, SyntaxOptions>(options);

            return options.Syntax;
        }
    }
}