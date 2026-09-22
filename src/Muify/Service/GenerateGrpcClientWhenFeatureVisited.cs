namespace Muify.Service
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis.CSharp;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using static Muify.Service.GenerateGrpcClientWhenFeatureVisited_Resources;

    internal sealed class GenerateGrpcClientWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature.Value.Metadata.HasGrpcClient || feature.Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Symbol request = (feature.Value.Name, Qualifier.Unqualified);
            Symbol response = ($"{feature.Value.Name}.Result", Qualifier.Unqualified);
            Symbol contract = ($"I{feature.Value.Name}Service", Qualifier.Unqualified);
            Symbol grpc = ($"I{feature.Value.Name}Service.IGrpc", Qualifier.Unqualified);
            Symbol options = (Name: "Options", Qualifier: feature.Namespace);
            Symbol scribe = (Name: "IScribe", Qualifier: "Mu.Communications.Tracing");

            Symbol task = Symbol.Undefined
                .Named((Name: "Task", Qualifier: "System.Threading.Tasks"))
                .WithArguments(response);

            Variable parameter = feature.Value.Name;

            var body = Snippet.From(
                Configuration.Options,
                $"_ = global::Ardalis.GuardClauses.Guard.Against.Null({parameter}, message: {SymbolDisplay.FormatLiteral(ObserveRequestRequired, quote: true)});",
                Snippet.Blank,
                $"using var scope = scribe.Next({parameter}, out global::Mu.Communications.Tracing.Ledger ledger);",
                Snippet.Blank,
                "var headers = new global::Grpc.Core.Metadata",
                "{",
                "    { global::Mu.Communications.Tracing.CallContextExtensions.CausationHeader, ledger.Causation.ToString(\"D\") },",
                "    { global::Mu.Communications.Tracing.CallContextExtensions.CorrelationHeader, ledger.Correlation.ToString(\"D\") },",
                "};",
                Snippet.Blank,
                "var callOptions = new global::Grpc.Core.CallOptions(",
                "    headers: headers,",
                "    deadline: global::System.DateTime.UtcNow.Add(options.Timeout),",
                "    cancellationToken: cancellationToken);",
                Snippet.Blank,
                "return await client",
                $"    .{feature.Value.Name}({parameter}, new global::ProtoBuf.Grpc.CallContext(callOptions))",
                "    .ConfigureAwait(false);");

            Class client = Class.Undefined
                .Implements(contract)
                .Named("Client")
                .WithMethods(method => method
                    .Accepts((feature.Value.Name, Type: request))
                    .Accepts((Name: "CancellationToken", Type: typeof(CancellationToken)))
                    .Named(feature.Value.Name)
                    .Returns(task)
                    .WithBody(body))
                .WithParameters((Name: "Client", Type: grpc))
                .WithParameters((Name: "Options", Type: options))
                .WithParameters((Name: "Scribe", Type: scribe));

            string content = Builder
                .New<Definition>()
                .For<Class>(@class => @class
                    .Containing(Class.Undefined
                        .Containing(client)
                        .IsStatic(true)
                        .Named("Grpc"))
                    .Named($"{feature.Value.Name}Service")
                    .WithExtensibility(Modifiers.Implicit)
                    .WithScope(Scopes.Unspecified))
                .From(feature.Namespace)
                .Referencing((Alias: string.Empty, Qualifier: "Ardalis.GuardClauses"))
                .ToSnippet(Configuration.Options);

            yield return new File(content, feature.Value.Name);
        }
    }
}