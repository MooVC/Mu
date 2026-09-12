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

            if (request is null)
            {
                return Feature.Undefined;
            }

            request.ParseFeatureMetadata(
                out INamedTypeSymbol @base,
                out INamedTypeSymbol mutation,
                out IOrderedEnumerable<IPropertySymbol> parameters,
                out IPropertySymbol[] results);

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
                    .HasConstructors(request.HasConstructors())
                    .HasFact(request.HasFact())
                    .HasRegistrar(request.HasRegistrar())
                    .IsPartial(request.IsPartial())
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