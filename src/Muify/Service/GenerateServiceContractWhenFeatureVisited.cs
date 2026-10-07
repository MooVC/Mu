namespace Muify.Service
{
    using System.Collections.Generic;
    using System.Threading;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Modelling;

    internal sealed class GenerateServiceContractWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature._Value.Metadata.HasServiceContract || feature._Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Symbol request = (feature._Value.Name, feature.Namespace);
            Symbol result = feature.GetResultSymbol();

            Symbol response = Symbol.Undefined
                .Named((Name: "Result", Qualifier: "Mu"))
                .WithArguments(result);

            Symbol task = Symbol.Undefined
                .Named((Name: "Task", Qualifier: "System.Threading.Tasks"))
                .WithArguments(response);

            string content = Builder
                .New<Definition>()
                .For<Interface>(@interface => @interface
                    .Named($"I{feature._Value.Name}Service")
                    .WithMethods(method => method
                        .Accepts((feature._Value.Name, Type: request))
                        .Accepts((Name: "CancellationToken", Type: typeof(CancellationToken)))
                        .Named(feature._Value.Name)
                        .Returns(task)))
                .From(feature.Namespace)
                .ToSnippet(Configuration.Options.WithTypes(types => types
                    .WithMethods(methods => methods.WithQualifications(types.Qualifications))));

            yield return new File(content, $"{feature.Namespace}.I{feature._Value.Name}Service");
        }
    }
}