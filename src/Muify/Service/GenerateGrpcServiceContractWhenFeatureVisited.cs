namespace Muify.Service
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Modelling;

    internal sealed class GenerateGrpcServiceContractWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature._Value.Metadata.HasGrpcServiceContract || feature._Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Symbol request = (feature._Value.Name, feature.Namespace);
            Symbol result = feature.GetResultSymbol();

            Symbol response = Symbol.Undefined
                .Named((Name: "Result", Qualifier: "Mu"))
                .WithArguments(result);

            Symbol task = Symbol.Undefined
                .Named((Name: "ValueTask", Qualifier: "System.Threading.Tasks"))
                .WithArguments(response);

            Interface contract = Interface.Undefined
                .AttributedWith(service => service
                    .Named((Name: "Service", Qualifier: "ProtoBuf.Grpc.Configuration"))
                    .WithArguments((Name: string.Empty, Value: $"\"{feature.Namespace}.Service\"")))
                .Named("IGrpc")
                .WithMethods(method => method
                    .Accepts((feature._Value.Name, Type: request))
                    .Accepts(context => context
                        .DefaultedTo("default")
                        .Named("Context")
                        .OfType((Name: "CallContext", Qualifier: "ProtoBuf.Grpc")))
                    .AttributedWith(operation => operation
                        .Named((Name: "Operation", Qualifier: "ProtoBuf.Grpc.Configuration"))
                        .WithArguments((Name: string.Empty, Value: $"\"{feature._Value.Name}\"")))
                    .Named(feature._Value.Name)
                    .Returns(task));

            string content = Builder
                .New<Definition>()
                .For<Interface>(@interface => @interface
                    .Containing(contract)
                    .Named($"I{feature._Value.Name}Service")
                    .WithScope(Scopes.Unspecified))
                .From(feature.Namespace)
                .ToSnippet(Configuration.Options.WithTypes(types => types
                    .WithMethods(methods => methods.WithQualifications(types.Qualifications))));

            yield return new File(content, $"I{feature._Value.Name}Service.Grpc");
        }
    }
}