namespace Muify.Service
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using static Muify.Service.GenerateGrpcServiceWhenFeatureVisited_Resources;

    internal sealed class GenerateGrpcServiceWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature.Value.Metadata.HasGrpcService || feature.Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Symbol contract = ($"I{feature.Value.Name}Service.IGrpc", Qualifier.Unqualified);
            Symbol extensions = (Name: "CallContextExtensions", Qualifier: "Mu.Communications.Tracing");
            Symbol guard = (Name: "Guard", Qualifier: "Ardalis.GuardClauses");
            Symbol ledger = (Name: "Ledger", Qualifier: "Mu.Communications.Tracing");
            Symbol manager = (Name: "IScopeManager", Qualifier: "Mu.Auditing");
            Symbol request = (feature.Value.Name, Qualifier.Unqualified);
            Symbol response = ($"{feature.Value.Name}.Result", Qualifier.Unqualified);
            Symbol scope = (Name: "Scope", Qualifier: "Mu.Auditing");
            Symbol scribe = (Name: "IScribe", Qualifier: "Mu.Communications.Tracing");

            Symbol handler = Symbol.Undefined
                .Named((Name: "IHandler", Qualifier: "Mu.Communications.Mediation"))
                .WithArguments(request, response);

            Symbol task = Symbol.Undefined
                .Named((Name: "ValueTask", Qualifier: "System.Threading.Tasks"))
                .WithArguments(response);

            string message = SymbolDisplay.FormatLiteral(ObserveRequestRequired, quote: true);
            Variable parameter = feature.Value.Name;

            Snippet body = Snippet
                .From(
                    Configuration.Options,
                    "return await handler",
                    $"    .Handle({parameter}, context.CancellationToken)",
                    "    .ConfigureAwait(false);")
                .Block(Configuration.Options, "using (scribe.Set(ledger))")
                .Block(Configuration.Options, $"using (manager.Begin({scope.ToSnippet(Configuration.Options)}.External))")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, $"{ledger.ToSnippet(Configuration.Options)} ledger = {extensions.ToSnippet(Configuration.Options)}.ToLedger(context, {parameter});")
                .Prepend(Configuration.Options, Snippet.Blank)
                .Prepend(Configuration.Options, $"_ = {guard.ToSnippet(Configuration.Options)}.Against.Null({parameter}, message: {message});");

            Class service = Class.Undefined
                .Implements(contract)
                .Named("Service")
                .WithMethods(method => method
                    .Accepts((feature.Value.Name, Type: request))
                    .Accepts(context => context
                        .DefaultedTo("default")
                        .Named("Context")
                        .OfType((Name: "CallContext", Qualifier: "ProtoBuf.Grpc")))
                    .Named(feature.Value.Name)
                    .Returns(task)
                    .WithBody(body))
                .WithParameters((Name: "Handler", Type: handler))
                .WithParameters((Name: "Manager", Type: manager))
                .WithParameters((Name: "Scribe", Type: scribe));

            string content = Builder
                .New<Definition>()
                .For<Class>(@class => @class
                    .Containing(Class.Undefined
                        .Containing(service)
                        .IsStatic(true)
                        .Named("Grpc"))
                    .Named($"{feature.Value.Name}Service")
                    .WithExtensibility(Modifiers.Implicit)
                    .WithScope(Scopes.Unspecified))
                .From(feature.Namespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{feature.Value.Name}Service.Grpc.Service");
        }
    }
}