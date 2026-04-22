namespace Muify
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using Mu.Modelling;

    [Generator(LanguageNames.CSharp)]
    public sealed partial class ModelGenerator
        : IIncrementalGenerator
    {
        private static readonly IServiceProvider _provider = new ServiceProvider();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValueProvider<Model> models = context.CompilationProvider.Select(GetModel);

            context.RegisterSourceOutput(models, Generate);
        }

        private static void Generate(SourceProductionContext context, Model model)
        {
            var navigator = new ModelNavigator(_provider);
            IEnumerable<File> results = navigator.Navigate<File>(model);

            foreach (File result in results)
            {
                var source = SourceText.From(result.Content, Encoding.UTF8);

                context.AddSource(result.Hint, source);
            }
        }

        private static Model GetModel(Compilation compilation, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}