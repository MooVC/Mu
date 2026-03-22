namespace Muify
{
    using Microsoft.CodeAnalysis;

    [Generator(LanguageNames.CSharp)]
    public sealed class EmbeddedAttributeGenerator
        : IIncrementalGenerator
    {
        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(Generate);
        }

        private static void Generate(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddEmbeddedAttributeDefinition();
        }
    }
}