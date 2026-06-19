namespace Muify.Service
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Result = MooVC.Syntax.CSharp.Result;

    internal sealed class FeatureRegistrarVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature.Value.Metadata.IsOutOfScope || feature.Value.Metadata.HasRegistrar)
            {
                yield break;
            }

            Symbol configuration = (Name: "IConfiguration", Qualifier: "Microsoft.Extensions.Configuration");
            Symbol container = (Name: "Container", Qualifier: "SimpleInjector");

            string content = Builder
                .New<Definition>()
                .For<Record>(record => record
                    .Implements((Name: "IRegistrar", Qualifier: "Mu.Composition"))
                    .Named(feature.Value.Name)
                    .WithMethods(register => register
                        .Accepts((Name: "Configuration", Type: configuration))
                        .Accepts((Name: "Container", Type: container))
                        .Named("Register")
                        .Returns(result => result
                            .OfType(container)
                            .WithMode(Result.Modes.Synchronous))
                        .WithExtensibility(Modifiers.Static)
                        .WithBody("return container;")))
                .From(feature.Namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, "Registrar");
        }
    }
}