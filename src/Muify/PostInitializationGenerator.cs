namespace Muify
{
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using Muify.Domain;
    using Muify.Service;

    [Generator(LanguageNames.CSharp)]
    public sealed class PostInitializationGenerator
        : IIncrementalGenerator
    {
        private static readonly IPostInitializationStrategy[] _strategies =
        {
            new UnitAttributeStrategy(),
            new CreationalAttributeStrategy(),
            new IdentityAttributeStrategy(),
            new NonMutationalStrategy(),
            new TransitionalAttributeStrategy(),
        };

        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(Generate);
        }

        private static void Generate(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddEmbeddedAttributeDefinition();

            foreach (IPostInitializationStrategy strategy in _strategies)
            {
                foreach (File file in strategy.Apply())
                {
                    context.AddSource(file.Hint, SourceText.From(file.Content, Encoding.UTF8));
                }
            }
        }
    }
}