namespace Muify.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Parameter = Mu.Modelling.Parameter;

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
            Parameter[] payload = feature.Value.Parameters.OrderBy(parameter => parameter.Name).ToArray();
            Parameter[] parameters = payload
                .Append((Name: "Identity", Type: typeof(Guid)))
                .Append((Name: "Proposed", Type: typeof(DateTimeOffset)))
                .OrderBy(parameter => parameter.Name)
                .ToArray();

            var content = new List<string>
            {
                $"namespace {feature.Namespace};",
                string.Empty,
                $"public sealed partial record {fact}",
                $"    : global::Mu.Modelling.Behavior.Fact<{Configuration.Render(unit)}>,",
                $"      global::Mu.Modelling.Behavior.IConvertFrom<{Configuration.Render(definition)}, {Configuration.Render(request)}>",
                "{",
            };

            AddConstructor(content, fact, payload, payload, serialization: false);
            content.Add(string.Empty);
            AddConstructor(content, fact, parameters, payload, serialization: true);

            foreach (Parameter parameter in payload)
            {
                content.Add(string.Empty);
                content.Add($"    public {Configuration.Render(parameter.Type)} {parameter.Name.ToSnippet(Identifier.Options.Pascal)} {{ get; init; }}");
            }

            string arguments = string.Join(", ", payload.Select(parameter => $"subject.{parameter.Name.ToSnippet(Identifier.Options.Pascal)}"));

            content.Add(string.Empty);
            content.Add($"    public static implicit operator {fact}({Configuration.Render(request)} subject)");
            content.Add("    {");
            content.Add($"        return new {fact}({arguments});");
            content.Add("    }");
            content.Add("}");

            yield return new File(content.ToSnippet(Configuration.Options), fact);
        }

        private static void AddConstructor(List<string> content, Name fact, IEnumerable<Parameter> parameters, IEnumerable<Parameter> payload, bool serialization)
        {
            if (serialization)
            {
                content.Add("    [global::System.Text.Json.Serialization.JsonConstructorAttribute]");
            }

            string arguments = string.Join(", ", parameters.Select(parameter => $"{Configuration.Render(parameter.Type)} {parameter.Name.ToSnippet(Variable.Options.Camel)}"));

            content.Add($"    internal {fact}({arguments})");

            if (serialization)
            {
                content.Add("        : base(identity, proposed)");
            }

            content.Add("    {");

            foreach (Variable name in payload.Select(parameter => parameter.Name))
            {
                content.Add($"        {name.ToSnippet(Identifier.Options.Pascal)} = {name.ToSnippet(Variable.Options.Camel)};");
            }

            content.Add("    }");
        }
    }
}