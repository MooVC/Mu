namespace Muify
{
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using MooVC.Syntax;

    [Generator(LanguageNames.CSharp)]
    public abstract class AttributeGenerator
        : IIncrementalGenerator
    {
        private readonly string _hint;
        private readonly string _metadata;

        private protected AttributeGenerator(string hint, string metadata)
        {
            _hint = hint;
            _metadata = metadata;
        }

        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(context.CompilationProvider, Generate);
        }

        protected abstract Snippet GetContent();

        private void Generate(SourceProductionContext context, Compilation compilation)
        {
            if (compilation.GetTypeByMetadataName(_metadata) is object)
            {
                return;
            }

            Snippet content = GetContent();

            context.AddSource(_hint, SourceText.From(content, Encoding.UTF8));
        }
    }
}