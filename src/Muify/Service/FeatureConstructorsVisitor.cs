namespace Muify.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Parameter = MooVC.Syntax.CSharp.Parameter;

    internal sealed class FeatureConstructorsVisitor
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (!feature.Value.Metadata.IsPartial || feature.Value.Metadata.HasConstructors || feature.Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Parameter[] payload = feature.Value.Parameters
                .OrderBy(parameter => parameter.Name)
                .Select(parameter => Parameter.Undefined.Named(parameter.Name).OfType(parameter.Type))
                .ToArray();

            var inherited = new List<Parameter>
            {
                (Name: "Identity", Type: typeof(Guid)),
                (Name: "Proposed", Type: typeof(DateTimeOffset)),
            };

            bool isTransitional = feature.Value.Type.IsMutational && feature.Value.Mutational.Type.IsTransitional;

            if (isTransitional)
            {
                Symbol identity = feature.Value.Metadata.TargetIdentity.IsUndefined
                    ? feature.Features.Unit.Value.Identity.GetSymbol(feature.Features.Unit.Namespace)
                    : feature.Value.Metadata.TargetIdentity;

                Symbol target = Symbol.Undefined
                    .Named((Name: "Reference", Qualifier: "Mu.Modelling.State"))
                    .WithArguments(identity);

                inherited.Add((Name: "Target", Type: target));
            }

            Parameter[] parameters = payload
                .Concat(inherited)
                .OrderBy(parameter => parameter.Name)
                .ToArray();

            Snippet[] arguments = inherited
                .Select(parameter => parameter.Name.ToSnippet(Variable.Options.Camel))
                .ToArray();

            var body = payload
                .Select(parameter => $"{parameter.Name.ToSnippet(Identifier.Options.Pascal)} = {parameter.Name.ToSnippet(Variable.Options.Camel)};")
                .ToSnippet(Configuration.Options);

            var content = Builder
                .New<Definition>()
                .For<Record>(record => record
                    .Named(feature.Value.Name)
                    .WithConstructors(constructor => constructor
                        .WithArguments(isTransitional ? "target: default" : string.Empty)
                        .WithScope(Scopes.Public))
                    .WithConstructors(constructor => constructor
                        .AttributedWith(attribute => attribute
                            .Named((Name: "JsonConstructorAttribute", Qualifier: "System.Text.Json.Serialization")))
                        .WithArguments(arguments)
                        .WithBody(body)
                        .WithParameters(parameters)
                        .WithScope(Scopes.Internal)))
                .From(feature.Namespace)
                .ImportReferences(feature.Namespace)
                .ToSnippet(Configuration.Options);

            string source = content;

            if (!isTransitional)
            {
                // Builder omits an empty public constructor as undefined, so remove its initializer after rendering.
                CompilationUnitSyntax declaration = SyntaxFactory.ParseCompilationUnit(source);
                ConstructorDeclarationSyntax constructor = declaration.DescendantNodes()
                    .OfType<ConstructorDeclarationSyntax>()
                    .Single(candidate => candidate.ParameterList.Parameters.Count == 0);

                source = declaration.ReplaceNode(constructor, constructor.WithInitializer(null)).ToFullString();
            }

            yield return new File(source, $"{feature.Value.Name}.ctor");
        }
    }
}