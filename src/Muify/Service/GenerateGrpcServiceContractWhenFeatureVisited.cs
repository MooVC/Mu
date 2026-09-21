namespace Muify.Service
{
    using System.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;

    internal sealed class GenerateGrpcServiceContractWhenFeatureVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature, File>
    {
        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature feature)
        {
            if (feature.Value.Metadata.HasGrpcServiceContract || feature.Value.Metadata.IsOutOfScope)
            {
                yield break;
            }

            Symbol request = (feature.Value.Name, Qualifier.Unqualified);
            Symbol response = ($"{feature.Value.Name}.Result", Qualifier.Unqualified);

            Symbol task = Symbol.Undefined
                .Named((Name: "ValueTask", Qualifier: "System.Threading.Tasks"))
                .WithArguments(response);

            Interface contract = Interface.Undefined
                .AttributedWith(service => service
                    .Named((Name: "Service", Qualifier: "ProtoBuf.Grpc.Configuration"))
                    .WithArguments((Name: string.Empty, Value: $"\"{feature.Namespace}.Service\"")))
                .Named("IGrpc")
                .WithMethods(method => method
                    .Accepts((Name: feature.Value.Name, Type: request))
                    .Accepts(context => context
                        .DefaultedTo("default")
                        .Named("Context")
                        .OfType((Name: "CallContext", Qualifier: "ProtoBuf.Grpc")))
                    .AttributedWith(operation => operation
                        .Named((Name: "Operation", Qualifier: "ProtoBuf.Grpc.Configuration"))
                        .WithArguments((Name: string.Empty, Value: $"\"{feature.Value.Name}\"")))
                    .Named(feature.Value.Name)
                    .Returns(task));

            string content = Builder
                .New<Definition>()
                .For<Interface>(@interface => @interface
                    .Containing(contract)
                    .Named($"I{feature.Value.Name}Service")
                    .WithScope(Scopes.Unspecified))
                .From(feature.Namespace)
                .ToSnippet(Configuration.Options.WithTypes(types => types
                    .WithMethods(methods => methods.WithQualifications(types.Qualifications))));

            yield return new File(content, feature.Value.Name);
        }
    }
}