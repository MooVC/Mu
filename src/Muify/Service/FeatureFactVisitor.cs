namespace Muify.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Conversion = MooVC.Syntax.CSharp.Conversion;
    using Parameter = MooVC.Syntax.CSharp.Parameter;

    internal sealed class FeatureFactVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature.Value.Metadata.HasFact || feature.Value.Mutational.Fact.IsUnnamed)
            {
                yield break;
            }

            Name fact = feature.Value.Mutational.Fact;
            Symbol request = (feature.Value.Name, Qualifier: feature.Namespace);
            Symbol unit = (feature.Features.Unit.Value.Name, Qualifier: feature.Features.Unit.Namespace);
            Symbol definition = (fact, Qualifier: feature.Namespace);

            Parameter[] payload = feature.Value.Parameters
                .OrderBy(parameter => parameter.Name)
                .Select(parameter => Parameter.Undefined.Named(parameter.Name).OfType(parameter.Type))
                .ToArray();

            Parameter[] parameters = payload
                .Append((Name: "Identity", Type: typeof(Guid)))
                .Append((Name: "Proposed", Type: typeof(DateTimeOffset)))
                .OrderBy(parameter => parameter.Name)
                .ToArray();

            var body = payload
                .Select(parameter => $"{parameter.Name.ToSnippet(Identifier.Options.Pascal)} = {parameter.Name.ToSnippet(Variable.Options.Camel)};")
                .ToSnippet(Configuration.Options);

            Constructor constructor = Constructor.Undefined
                .WithBody(body)
                .WithScope(Scopes.Internal);

            Property[] properties = payload
                .Select(parameter => Property.Undefined
                    .Named(parameter.Name.ToSnippet(Identifier.Options.Pascal).ToString())
                    .OfType(parameter.Type))
                .ToArray();

            string arguments = string.Join(", ", payload.Select(parameter => $"subject.{parameter.Name.ToSnippet(Identifier.Options.Pascal)}"));

            var content = Builder
                .New<Definition>()
                .For<Record>(record => record
                    .DerivesFrom(@base => @base
                        .Named((Name: "Fact", Qualifier: "Mu.Modelling.Behavior"))
                        .WithGenerics(unit))
                    .Implements(@interface => @interface
                        .Named((Name: "IConvertFrom", Qualifier: "Mu.Modelling.Behavior"))
                        .WithArguments(definition, request))
                    .Named(fact)
                    .WithConstructors(
                        constructor.WithParameters(payload),
                        constructor
                            .AttributedWith(attribute => attribute
                                .Named((Name: "JsonConstructorAttribute", Qualifier: "System.Text.Json.Serialization")))
                            .WithArguments("identity", "proposed")
                            .WithParameters(parameters))
                    .WithOperators(operators => operators
                        .WithConversions(conversion => conversion
                            .ForType(request)
                            .WithBody(Snippet.From(Configuration.Options, $"return new {fact}({arguments});"))
                            .WithDirection(Conversion.Intents.From)
                            .WithMode(Conversion.Types.Implicit)))
                    .WithProperties(properties))
                .From(feature.Namespace)
                .ImportReferences(feature.Namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, fact);
        }
    }
}