namespace Muify.Service
{
    using System.Collections.Generic;
    using System.Threading;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal sealed class GenerateServiceContractWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature.Value.Metadata.HasServiceContract || feature.Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Symbol request = (feature.Value.Name, Qualifier.Unqualified);
            Symbol response = ($"{feature.Value.Name}.Result", Qualifier.Unqualified);

            Symbol task = Symbol.Undefined
                .Named((Name: "Task", Qualifier: "System.Threading.Tasks"))
                .WithArguments(response);

            string content = Builder
                .New<Definition>()
                .For<Interface>(@interface => @interface
                    .Named($"I{feature.Value.Name}Service")
                    .WithMethods(method => method
                        .Accepts((feature.Value.Name, Type: request))
                        .Accepts((Name: "CancellationToken", Type: typeof(CancellationToken)))
                        .Named(feature.Value.Name)
                        .Returns(task)))
                .From(feature.Namespace)
                .ToSnippet(Configuration.Options.WithTypes(types => types
                    .WithMethods(methods => methods.WithQualifications(types.Qualifications))));

            yield return new File(content, feature.Value.Name);
        }
    }
}