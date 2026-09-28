namespace Muify.Service
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis.CSharp;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using static Muify.Service.GenerateServiceWhenFeatureVisited_Resources;

    internal sealed class GenerateServiceWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature.Value.Metadata.HasService || feature.Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Symbol request = (feature.Value.Name, Qualifier.Unqualified);
            Symbol response = ($"{feature.Value.Name}.Result", Qualifier.Unqualified);
            Symbol contract = ($"I{feature.Value.Name}Service", Qualifier.Unqualified);
            Symbol guard = (Name: "Guard", Qualifier: "Ardalis.GuardClauses");

            Symbol handler = Symbol.Undefined
                .Named((Name: "IHandler", Qualifier: "Mu.Communications.Mediation"))
                .WithArguments(request, response);

            Symbol manager = (Name: "IScopeManager", Qualifier: "Mu.Auditing");
            Symbol scope = (Name: "Scope", Qualifier: "Mu.Auditing");
            Symbol scribe = (Name: "IScribe", Qualifier: "Mu.Communications.Tracing");

            Symbol task = Symbol.Undefined
                .Named((Name: "Task", Qualifier: "System.Threading.Tasks"))
                .WithArguments(response);

            Variable parameter = feature.Value.Name;

            Snippet body = Snippet
                .From(
                    Configuration.Options,
                    "return await handler",
                    $"    .Handle({parameter}, cancellationToken)",
                    "    .ConfigureAwait(false);")
                .Block(Configuration.Options, $"using (scribe.Next({parameter}, out _))")
                .Block(Configuration.Options, $"using (manager.Begin({scope.ToSnippet(Configuration.Options)}.Internal))")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(
                    Configuration.Options,
                    $"_ = {guard.ToSnippet(Configuration.Options)}.Against.Null({parameter}, message: {SymbolDisplay.FormatLiteral(ObserveRequestRequired, quote: true)});");

            string content = Builder
                .New<Definition>()
                .For<Class>(@class => @class
                    .Implements(contract)
                    .Named($"{feature.Value.Name}Service")
                    .WithMethods(method => method
                        .Accepts((feature.Value.Name, Type: request))
                        .Accepts((Name: "CancellationToken", Type: typeof(CancellationToken)))
                        .Named(feature.Value.Name)
                        .Returns(task)
                        .WithBody(body))
                    .WithParameters((Name: "Handler", Type: handler))
                    .WithParameters((Name: "Manager", Type: manager))
                    .WithParameters((Name: "Scribe", Type: scribe)))
                .From(feature.Namespace)
                .Referencing((Alias: string.Empty, Qualifier: "Ardalis.GuardClauses"))
                .ToSnippet(Configuration.Options);

            yield return new File(content, feature.Value.Name);
        }
    }
}