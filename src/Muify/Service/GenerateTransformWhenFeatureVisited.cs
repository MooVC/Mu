namespace Muify.Service
{
    using System.Collections.Generic;
    using System.Linq;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Attribute = Mu.Modelling.Attribute;
    using Parameter = Mu.Modelling.Parameter;
    using Result = MooVC.Syntax.CSharp.Result;

    internal sealed class GenerateTransformWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature._Value.Type.IsNonMutational
             || !feature._Value.Metadata.Transforms.IsDefaultOrEmpty
             || feature._Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Symbol aggregate = (feature.Features.Unit._Value.Name, feature.Features.Unit.Namespace);
            Symbol fact = (feature._Value.Mutational.Fact, feature.Namespace);

            string[] assignments = feature.Features.Unit._Value.Attributes
                .Where(attribute => feature._Value.Parameters.Any(parameter => IsMatch(attribute, parameter)))
                .Select(attribute => $"{attribute.Name} = fact.{attribute.Name},")
                .ToArray();

            Snippet body = Snippet
                .From(Configuration.Options, assignments)
                .Block(Configuration.Options, "return aggregate with")
                .Append(';');

            string content = Builder
                .New<Definition>()
                .For<Class>(@class => @class
                    .DerivesFrom(@base => @base
                        .Named((Name: "ITransform", Qualifier: "Mu.Modelling.Services"))
                        .WithGenerics(aggregate, fact))
                    .IsPartial(false)
                    .Named("Transform")
                    .WithMethods(apply => apply
                        .Accepts((Name: "Aggregate", Type: aggregate))
                        .Accepts((Name: "Fact", Type: fact))
                        .Named("Apply")
                        .Returns(result => result
                            .OfType(aggregate)
                            .WithMode(Result.Modes.Synchronous))
                        .WithBody(body))
                    .WithScope(Scopes.Internal))
                .From(feature.Namespace)
                .Referencing(feature.Features.Unit.Namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{feature.Namespace}.Transform");
        }

        private static bool IsMatch(Attribute attribute, Parameter parameter)
        {
            return parameter.Name.Equals(attribute.Name.ToString())
                && parameter.Type == attribute.Type;
        }
    }
}