namespace Muify.Service
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis.CSharp;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Modelling;
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

            Symbol context = (Name: "CallContext", Qualifier: "ProtoBuf.Grpc");
            Symbol options = (Name: "CallOptions", Qualifier: "Grpc.Core");
            Symbol contract = ($"I{feature.Value.Name}Service", Qualifier.Unqualified);
            Symbol dateTime = (Name: "DateTime", Qualifier: "System");
            Symbol extensions = (Name: "CallContextExtensions", Qualifier: "Mu.Communications.Tracing");
            Symbol grpc = ($"I{feature.Value.Name}Service.IGrpc", Qualifier.Unqualified);
            Symbol guard = (Name: "Guard", Qualifier: "Ardalis.GuardClauses");
            Symbol ledger = (Name: "Ledger", Qualifier: "Mu.Communications.Tracing");
            Symbol metadata = (Name: "Metadata", Qualifier: "Grpc.Core");
            Symbol request = (feature.Value.Name, feature.Namespace);
            Symbol result = feature.GetResultSymbol();

            Symbol response = Symbol.Undefined
                .Named((Name: "Result", Qualifier: "Mu"))
                .WithArguments(result);
            Symbol scribe = (Name: "IScribe", Qualifier: "Mu.Communications.Tracing");

            Symbol task = Symbol.Undefined
                .Named((Name: "Task", Qualifier: "System.Threading.Tasks"))
                .WithArguments(response);

            Variable parameter = feature.Value.Name;

            var body = Snippet.From(
                Configuration.Options,
                $"_ = {guard.ToSnippet(Configuration.Options)}.Against.Null({parameter}, message: {SymbolDisplay.FormatLiteral(ObserveRequestRequired, quote: true)});",
                Snippet.Blank,
                $"using var scope = scribe.Next({parameter}, out {ledger.ToSnippet(Configuration.Options)} ledger);",
                Snippet.Blank,
                $"var headers = new {metadata.ToSnippet(Configuration.Options)}",
                "{",
                $"    {{ {extensions.ToSnippet(Configuration.Options)}.CausationHeader, ledger.Causation.ToString(\"D\") }},",
                $"    {{ {extensions.ToSnippet(Configuration.Options)}.CorrelationHeader, ledger.Correlation.ToString(\"D\") }},",
                "};",
                Snippet.Blank,
                $"var callOptions = new {options.ToSnippet(Configuration.Options)}(",
                "    headers: headers,",
                $"    deadline: {dateTime.ToSnippet(Configuration.Options)}.UtcNow.Add(timeout),",
                "    cancellationToken: cancellationToken);",
                Snippet.Blank,
                "return await client",
                $"    .{feature.Value.Name}({parameter}, new {context.ToSnippet(Configuration.Options)}(callOptions))",
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
                .WithParameters((Name: "Scribe", Type: scribe))
                .WithParameters((Name: "Timeout", Type: typeof(TimeSpan)));

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

            yield return new File(content, $"{feature.Value.Name}Service.Grpc.Client");
        }
    }
}