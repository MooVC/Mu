namespace Muify.Semantics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using MooVC;
    using MooVC.Syntax;
    using Mu.Modelling;
    using Muify.Service;

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
            IOrderedEnumerable<IPropertySymbol> parameters = request
                .GetProperties(property => property.DeclaredAccessibility == Accessibility.Public
                    && !property.IsStatic
                    && !property.IsIndexer
                    && property.SetMethod is object)
                .OrderBy(property => property.Name, StringComparer.Ordinal);

            INamedTypeSymbol mutation = request
                .GetAttributes()
                .Select(attribute => attribute.AttributeClass)
                .FirstOrDefault(attribute => attribute.IsMutationalAttribute());

            Feature feature = Feature.Undefined
                .Named(names.Feature)
                .Enumerate(
                    (parameter, subject) => subject.Using(member => member
                        .Named(parameter.Name)
                        .OfType(parameter.Type.ToSyntax())),
                    parameters)
                .Enumerate((result, subject) => result.CreateResult(subject), results)
                .WithMetadata(metadata => metadata
                    .Enumerate((reference, subject) => subject.WithReferences(reference), request.GetReferences())
                    .Enumerate((registrar, subject) => subject.WithRegistrars(registrar), request.ContainingNamespace.GetRegistrars())
                    .Enumerate((transform, subject) => subject.WithTransforms(transform), request.GetTransforms())
                    .HasBase(request.HasUseCaseBase())
                    .HasBinder(request.HasBinder())
                    .HasFact(request.HasFact())
                    .HasRegistrar(request.HasRegistrar())
                    .IsPartial(request.IsPartial()));

            if (mutation is null)
            {
                return feature;
            }

            return feature.IsMutational(mutational => mutational
                .Raises(mutation.TypeArguments[0].Name)
                .OfType(mutation.Name == $"{CreationalAttributeStrategy.Name}Attribute"
                    ? Mutational.Kinds.Creational
                    : Mutational.Kinds.Transitional));
        }
    }
}