namespace Muify.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Mu.Modelling.Syntax.CSharp;
    using Conversion = MooVC.Syntax.CSharp.Conversion;

    internal sealed class FeatureFactVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature.Value.Metadata.HasFact)
            {
                yield break;
            }

            string arguments = string.Join(", ", feature.Value.Parameters.Select(parameter => $"subject.{parameter.Name}"));
            Snippet assignments = feature.Value.Parameters.ToAssignments(Configuration.Options);
            Name fact = feature.Value.Mutational.Fact;
            Symbol request = (feature.Value.Name, Qualifier: feature.Namespace);
            Symbol unit = (feature.Features.Unit.Value.Name, Qualifier: feature.Features.Unit.Namespace);

            string content = Builder
                .New<Definition>()
                .For<Record>(record => record
                    .DerivesFrom(@base => @base
                        .Named((Moniker: "Fact", Qualifier: "Mu.Modelling.Behavior"))
                        .WithArguments(unit))
                    .Named(fact)
                    .WithConstructors(@default => @default
                        .Enumerate((current, subject) => subject.WithParameters(parameter => parameter.From(current)), feature.Value.Parameters)
                        .WithBody(assignments))
                    .WithConstructors(serialization => serialization
                        .AttributedWith(attribute => attribute
                            .Named((Name: "JsonConstructorAttribute", Qualifier: "System.Text.Json.Serialization")))
                        .Enumerate((current, subject) => subject.WithParameters(parameter => parameter.From(current)), feature.Value.Parameters)
                        .WithBody(assignments)
                        .WithParameters((Name: "Identity", Type: typeof(Guid)))
                        .WithParameters((Name: "Proposed", Type: typeof(DateTimeOffset))))
                    .WithProperties(feature.Value.Parameters)
                    .WithOperators(operators => operators
                        .WithConversions(conversion => conversion
                            .ForType(request)
                            .WithBody($"return new {fact}({arguments});")
                            .WithDirection(Conversion.Intents.From)
                            .WithMode(Conversion.Types.Implicit))))
                .From(feature.Namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, feature.Value.Mutational.Fact);
        }
    }
}