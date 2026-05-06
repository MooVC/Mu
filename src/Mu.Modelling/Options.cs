namespace Mu.Modelling
{
    using Ardalis.GuardClauses;
    using Fluentify;
    using MooVC.Syntax.Validation;
    using SyntaxOptions = MooVC.Syntax.CSharp.Options;

    [Fluentify]
    public sealed partial class Options
    {
        public static readonly Options Default = new Options();

        public GithubOptions Github { get; internal set; } = GithubOptions.Default;

        public SyntaxOptions Syntax { get; internal set; } = SyntaxOptions.Default;

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