namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Service;

    internal static partial class CompilationExtensions
    {
        public static Feature ParseFeatureModel(this Compilation compilation, (Name Area, Name Feature, Name Unit) names)
        {
            INamedTypeSymbol request = compilation.GetTypeByMetadataName($"{compilation.AssemblyName}.{names.Feature}");

            if (request is null || !SymbolEqualityComparer.Default.Equals(request.ContainingAssembly, compilation.Assembly))
            {
                return Feature.Undefined;
            }

            request.ParseFeatureMetadata(
                out INamedTypeSymbol @base,
                out INamedTypeSymbol mutation,
                out IOrderedEnumerable<IPropertySymbol> parameters,
                out IPropertySymbol[] results);

            INamedTypeSymbol service = request.ContainingNamespace.GetTypeMembers($"{request.Name}Service").FirstOrDefault();
            INamedTypeSymbol contract = request.ContainingNamespace.GetTypeMembers($"I{request.Name}Service").FirstOrDefault();
            INamedTypeSymbol grpc = service?.GetTypeMembers("Grpc").FirstOrDefault();
            ITypeSymbol fact = mutation?.TypeArguments.FirstOrDefault();

            ITypeSymbol aggregate = GetAggregate(names, request, @base);

            Feature feature = Feature.Undefined
                .Named(names.Feature)
                .Enumerate(AddParameter, parameters)
                .Enumerate((result, subject) => result.CreateResult(subject), results)
                .WithMetadata(metadata => metadata
                    .Enumerate((reference, subject) => subject.WithReferences(reference), request.GetReferences())
                    .Enumerate((invariant, subject) => subject.WithInvariants(invariant), request.GetInvariants(aggregate))
                    .Enumerate((registrar, subject) => subject.WithRegistrars(registrar), request.ContainingNamespace.GetRegistrars(includeDescendants: true))
                    .Enumerate((transform, subject) => subject.WithTransforms(transform), request.GetTransforms(aggregate, fact))
                    .HasBase(request.HasUseCaseBase())
                    .HasBinder(request.HasBinder())
                    .HasConstructors(request.HasConstructors())
                    .HasFact(request.HasFact())
                    .HasGrpcClient(grpc?.GetTypeMembers("Client").Any() == true)
                    .HasGrpcService(grpc?.GetTypeMembers("Service").Any() == true)
                    .HasGrpcServiceContract(contract?.GetTypeMembers("IGrpc").Any() == true)
                    .HasRegistrar(request.HasRegistrar())
                    .HasService(service is object)
                    .HasServiceContract(contract is object)
                    .IsPartial(request.IsPartial())
                    .WithHandler(request.GetImplementation("IHandler`2", "Mu.Communications.Mediation"))
                    .WithRoot(request.GetRoot(aggregate))
                    .WithService(request.GetImplementation("IService`2", "Mu.Modelling.Services"))
                    .WithTargetIdentity(@base.GetTargetIdentity()));

            if (@base?.Name == "Query" || (@base is null && mutation is null))
            {
                return feature.IsNonMutational();
            }

            bool isCreational = @base.IsCreational(mutation);

            return feature.IsMutational(mutational => mutational
                .Raises(mutation.GetFactName())
                .OfType(isCreational
                    ? Mutational.Kinds.Creational
                    : Mutational.Kinds.Transitional));
        }

        private static Feature AddParameter(IPropertySymbol parameter, Feature subject)
        {
            return subject.Using(member => member
                .Named(parameter.Name)
                .OfType(parameter.Type.ToSyntax()));
        }

        private static ITypeSymbol GetAggregate((Name Area, Name Feature, Name Unit) names, INamedTypeSymbol request, INamedTypeSymbol @base)
        {
            return @base?.TypeArguments[0]
                ?? request.ContainingNamespace.ContainingNamespace
                    .GetTypeMembers(names.Unit.ToString())
                    .FirstOrDefault();
        }

        private static string GetFactName(this INamedTypeSymbol mutation)
        {
            const int ExpectedArgumentsForMutationalAttribute = 1;

            return mutation is object && mutation.TypeArguments.Length == ExpectedArgumentsForMutationalAttribute
                ? mutation.TypeArguments[0].Name
                : string.Empty;
        }

        private static Symbol GetTargetIdentity(this INamedTypeSymbol @base)
        {
            return @base?.Name == TransitionalAttributeStrategy.Name
                ? @base.TypeArguments[1].ToSyntax()
                : Symbol.Undefined;
        }

        private static bool IsCreational(this INamedTypeSymbol @base, INamedTypeSymbol mutation)
        {
            return @base is object
                ? @base.Name == CreationalAttributeStrategy.Name
                : mutation.Name == $"{CreationalAttributeStrategy.Name}Attribute";
        }

        private static void ParseFeatureMetadata(
            this INamedTypeSymbol request,
            out INamedTypeSymbol @base,
            out INamedTypeSymbol mutation,
            out IOrderedEnumerable<IPropertySymbol> parameters,
            out IPropertySymbol[] results)
        {
            @base = request.GetFeatureBase();

            mutation = request
                .GetAttributes()
                .Select(attribute => attribute.AttributeClass)
                .FirstOrDefault(attribute => attribute.IsMutationalAttribute());

            parameters = request
                .GetProperties(property => property.DeclaredAccessibility == Accessibility.Public
                    && !property.IsStatic
                    && !property.IsIndexer
                    && property.SetMethod is object)
                .OrderBy(property => property.Name, StringComparer.Ordinal);

            results = request.GetResults();
        }
    }
}