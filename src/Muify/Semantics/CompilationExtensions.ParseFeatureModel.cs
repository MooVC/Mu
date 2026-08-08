namespace Muify.Semantics
{
    using Microsoft.CodeAnalysis;
    using MooVC;
    using MooVC.Syntax;
    using Mu.Modelling;

    internal static partial class CompilationExtensions
    {
        public static Feature ParseFeatureModel(this Compilation compilation, (Name Area, Name Feature, Name Unit) names)
        {
            INamedTypeSymbol request = compilation.GetTypeByMetadataName($"{compilation.AssemblyName}.{names.Feature}");

            if (request is null)
            {
                return Feature.Undefined;
            }

            IPropertySymbol[] results = request.GetResults();

            return Feature.Undefined
                .Named(names.Feature)
                .Enumerate((result, feature) => result.CreateResult(feature), results)
                .WithMetadata(metadata => metadata
                    .Enumerate((reference, subject) => subject.WithReferences(reference), request.GetReferences())
                    .Enumerate((registrar, subject) => subject.WithRegistrars(registrar), request.ContainingNamespace.GetRegistrars())
                    .Enumerate((transform, subject) => subject.WithTransforms(transform), request.GetTransforms())
                    .HasBase(request.HasUseCaseBase())
                    .HasBinder(request.HasBinder())
                    .HasFact(request.HasFact())
                    .HasRegistrar(request.HasRegistrar())
                    .IsPartial(request.IsPartial()));
        }
    }
}